using Carter;
using FluentValidation;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Extensions;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Unit.AddUnit;
using static HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Unit.UpdateUnit;

namespace HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Unit
{
    public static class UpdateUnit
    {
        public class UpdateUnitRequest : IRequest<Shared.Result>
        {
            public Guid Id { get; set; }
            public Guid departmentId { get; set; }
            public Guid? unitHeadId { get; set; }
            public Guid directorateId { get; set; }
            public string unitName { get; set; }
        }

        public class Validator : AbstractValidator<UpdateUnitRequest>
        {
            private readonly IServiceScopeFactory _scopeFactory;
            public Validator(IServiceScopeFactory scopeFactory)
            {
                _scopeFactory = scopeFactory;
                RuleFor(c => c.unitName).NotEmpty()
                    .MustAsync(async (model, name, cancellationToken) =>
                    {
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var dbContext = scope.ServiceProvider.GetService<HRMDBContext>();
                            var exist = await dbContext.Unit.AnyAsync(s => s.unitName == name && s.Id != model.Id, cancellationToken);
                            return !exist;
                        }
                    }).WithMessage("Unit Name Already Exist")
                    ;
                RuleFor(c => c.unitName)
                    .NotEmpty();
                RuleFor(c => c.unitName)
                .NotEmpty();
                RuleFor(c => c.directorateId)
                .NotEmpty();
                RuleFor(c => c.departmentId)
                .NotEmpty();

            }
        }

        public class Handler : IRequestHandler<UpdateUnitRequest, Shared.Result>
        {
            private readonly HRMDBContext _dbContext;
            private readonly IValidator<UpdateUnitRequest> _validator;
            public Handler(HRMDBContext dbContext, IValidator<UpdateUnitRequest> validator)
            {
                _dbContext = dbContext;
                _validator = validator;
            }
            public async Task<Shared.Result> Handle(UpdateUnitRequest request, CancellationToken cancellationToken)
            {
                var validationResponse = await _validator.ValidateAsync(request);

                if (validationResponse.IsValid is false)
                {
                    return Shared.Result.Failure<Guid>(Error.ValidationError(validationResponse));
                }
                var departmentHeadExist = await _dbContext.Department.AnyAsync(x => x.Id == request.departmentId);
                if (departmentHeadExist is false) return Shared.Result.Failure<Guid>(Error.CreateNotFoundError("Department Not Found"));
                
                var affectedRows = await _dbContext.Unit.Where(x => x.Id == request.Id)
                    .ExecuteUpdateAsync(setters =>
                          setters.SetProperty(p => p.updatedAt, DateTime.UtcNow)
                          .SetProperty(p => p.unitName, request.unitName)
                          .SetProperty(p => p.departmentId, request.departmentId)
                          .SetProperty(p => p.unitHeadId, request.unitHeadId)
                          .SetProperty(p => p.directorateId, request.directorateId)
                    ); ;
                if (affectedRows == 0) return Shared.Result.Failure<Guid>(Error.CreateNotFoundError("Unit Not Found"));

                return Shared.Result.Success();
            }
        }


    }
}

public class MapUpdateClubEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("api/unit/{Id}", async (ISender sender, Guid Id, AddUnitRequest request) =>
        {
            var response = await sender.Send(
                new UpdateUnitRequest
                {
                    Id = Id,
                    unitName = request.unitName,
                    departmentId = request.departmentId,
                    directorateId = request.directorateId,
                    unitHeadId = request.unitHeadId
                }
                );

            if (response.IsFailure)
            {
                return Results.UnprocessableEntity(response.Error);
            }

            if (response.IsSuccess)
            {
                return Results.NoContent();
            }

            return Results.BadRequest();

        }).WithTags("Setup-Unit")
              .WithMetadata(new ProducesResponseTypeAttribute(typeof(Guid), StatusCodes.Status200OK))
              .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest))
              .WithGroupName(SwaggerDoc.SwaggerEndpointDefintions.Setup)
              ;
    }
}
