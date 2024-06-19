using Carter;
using FluentValidation;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Grade.AddGrade;
using static HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Grade.UpdateGrade;

namespace HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Grade
{
    public class UpdateGrade
    {

        public record UpdatedGradeRequest : IRequest<Shared.Result>
        {
            public Guid Id { get; set; }
            public Guid categoryId { get; set; } = Guid.Empty;
            public string gradeName { get; set; } = String.Empty;
            public string level { get; set; }
            public ScaleEnum? scale { get; set; }
            public Double marketPremium { get; set; }
            public int minimunStep { get; set; }
            public int maximumStep { get; set; }
        }

        public class Validator : AbstractValidator<UpdatedGradeRequest>
        {
            private readonly IServiceScopeFactory _scopeFactory;
            public Validator(IServiceScopeFactory scopeFactory)
            {
                _scopeFactory = scopeFactory;
                RuleFor(c => c.gradeName).NotEmpty()
                    .MustAsync(async (model, name, cancellationToken) =>
                    {
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var dbContext = scope.ServiceProvider.GetService<HRMDBContext>();
                            var exist = await dbContext.Grade.AnyAsync(s => s.gradeName == name && s.Id != model.Id, cancellationToken);
                            return !exist;
                        }
                    }).WithMessage("Grade Name Already Exist")
                    ;
                RuleFor(c => c.categoryId)
                    .NotEmpty();
                RuleFor(c => c.level)
                .NotEmpty();
                RuleFor(c => c.scale).NotEmpty().IsInEnum();
                RuleFor(a => a.minimunStep).Must(x => int.TryParse(x.ToString(), out var val) && val > 0)
                    .WithMessage("Invalid Minimun Step");
                RuleFor(a => a.maximumStep).Must(x => int.TryParse(x.ToString(), out var val) && val > 0)
                .WithMessage("Invalid Maximum Step");
                RuleFor(c => c.marketPremium)
                    .Must(x => Double.TryParse(x.ToString(), out var val) && val > 0)
                    .WithMessage("Invalid Market Premium");

            }
        }

        internal sealed class Handler : IRequestHandler<UpdatedGradeRequest, Shared.Result>
        {
            private readonly HRMDBContext _dbContext;
            private readonly IValidator<UpdatedGradeRequest> _validator;
            public Handler(HRMDBContext dbContext, IValidator<UpdatedGradeRequest> validator)
            {

                _dbContext = dbContext;
                _validator = validator;

            }
            public async Task<Shared.Result> Handle(UpdatedGradeRequest request, CancellationToken cancellationToken)
            {
                var validationResponse = await _validator.ValidateAsync(request);

                if (validationResponse.IsValid is false)
                {
                    return Shared.Result.Failure(Error.ValidationError(validationResponse));
                }

                var affectedRows = await _dbContext.Grade.Where(x => x.Id == request.Id).ExecuteUpdateAsync(setters =>
                    setters.SetProperty(c => c.marketPremium, request.marketPremium)
                    .SetProperty(c => c.updatedAt, DateTime.UtcNow)
                    .SetProperty(c => c.minimunStep, request.minimunStep)
                    .SetProperty(c => c.maximumStep, request.maximumStep)
                    .SetProperty(c => c.marketPremium, request.marketPremium)
                    .SetProperty(c => c.categoryId, request.categoryId)
                    .SetProperty(c => c.scale, request.scale.ToString())
                    .SetProperty(c => c.marketPremium, request.marketPremium)
                    .SetProperty(c => c.level, request.level)
                );

                if (affectedRows >= 1) return Shared.Result.Success();

                return Shared.Result.Failure(Error.CreateNotFoundError("Category To Update Not Found"));
            }
        }
    }
}

public class MapUpdateCommunityEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("api/grade/{Id}", async (ISender sender, Guid Id, AddGradeRequest request) =>
        {
            var response = await sender.Send(
                new UpdatedGradeRequest
                {
                    Id = Id,
                    categoryId = request.categoryId,
                    gradeName = request.gradeName,
                    level = request.level,
                    scale = request.scale,
                    maximumStep = request.maximumStep,
                    minimunStep = request.minimunStep,
                    marketPremium = request.marketPremium
                }
                ); ;

            if (response.IsFailure)
            {
                return Results.NotFound(response.Error);
            }
            if (response.IsSuccess)
            {
                return Results.NoContent();
            }
            return Results.BadRequest();


        }).WithTags("Setup-Grade")
              .WithMetadata(new ProducesResponseTypeAttribute(typeof(Guid), StatusCodes.Status204NoContent))
              .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest));
    }
}
