using AutoMapper;
using Carter;
using FluentValidation;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Grade.AddGrade;

namespace HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Grade
{
    public static class AddGrade
    {
        public enum ScaleEnum
        {
            SSPS,
            HSSS
        }

        public record AddGradeRequest : IRequest<Result<Guid>>
        {
            public Guid categoryId { get; set; } = Guid.Empty;
            public string gradeName { get; set; } = String.Empty;
            public string level { get; set; }
            public ScaleEnum? scale { get; set; }
            public Double marketPremium { get; set; }
            public int minimunStep { get; set; }
            public int maximumStep { get; set; }
        }

        public class Validator : AbstractValidator<AddGradeRequest>
        {
            private readonly IServiceScopeFactory _scopeFactory;
            public Validator(IServiceScopeFactory scopeFactory)
            {
                _scopeFactory = scopeFactory;
                RuleFor(c => c.gradeName).NotEmpty()
                    .MustAsync(async (name, cancellationToken) =>
                    {
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var dbContext = scope.ServiceProvider.GetService<HRMDBContext>();
                            var exist = await dbContext.Grade.AnyAsync(s => s.gradeName == name, cancellationToken);
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

        public class Hanlder : IRequestHandler<AddGradeRequest, Result<Guid>>
        {
            private readonly HRMDBContext _dbContext;
            private readonly IValidator<AddGradeRequest> _validator;
            private readonly IMapper _mapper;
            public Hanlder(HRMDBContext dbContext, IValidator<AddGradeRequest> validator, IMapper mapper)
            {

                _dbContext = dbContext;
                _validator = validator;
                _mapper = mapper;

            }
            public async Task<Result<Guid>> Handle(AddGradeRequest request, CancellationToken cancellationToken)
            {
                var validationResponse = await _validator.ValidateAsync(request, cancellationToken);

                if (validationResponse.IsValid is false)
                {
                    return Shared.Result.Failure<Guid>(Error.ValidationError(validationResponse));
                }

                var newGradeEntry = _mapper.Map<Entities.Grade>(request);
                newGradeEntry.createdAt = DateTime.UtcNow;
                newGradeEntry.updatedAt = DateTime.UtcNow;

                _dbContext.Add(newGradeEntry);
                await _dbContext.SaveChangesAsync();

                return Shared.Result.Success(newGradeEntry.Id);
            }
        }
    }
}

public class MapAddGradeEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/grade", async (ISender sender, AddGradeRequest request) =>
        {
            var response = await sender.Send(request);
            if (response.IsFailure)
            {
                return Results.UnprocessableEntity(response.Error);
            }

            return Results.Ok(response.Value);

        }).WithTags("Setup-Grade")
              .WithMetadata(new ProducesResponseTypeAttribute(StatusCodes.Status200OK))
              .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest))

          ;
    }
}