using Carter;
using FluentValidation;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Specialty.AddSpecialty;
using static HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Specialty.EditSpeciality;

namespace HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Specialty
{
    public static class EditSpeciality
    {

        public class EditSpecialityRequest : IRequest<Shared.Result>
        {
            public Guid Id { get; set; }
            public Guid categoryId { get; set; }
            public string specialityName { get; set; } = String.Empty;
        }

        public class Validator : AbstractValidator<EditSpecialityRequest>
        {

            private readonly IServiceScopeFactory _scopeFactory;
            public Validator(IServiceScopeFactory scopeFactory)
            {
                _scopeFactory = scopeFactory;
                RuleFor(c => c.specialityName).NotEmpty()
                    .MustAsync(async (model, name, cancellationToken) =>
                    {
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var dbContext = scope.ServiceProvider.GetService<HRMDBContext>();
                            var exist = await dbContext.Speciality.AnyAsync(s => s.specialityName == name && s.Id != model.Id, cancellationToken);
                            return !exist;
                        }
                    }).WithMessage("Speciality Name Already Exist")
                    ;
                RuleFor(c => c.categoryId).NotEmpty();

            }
        }

        internal sealed class Handler : IRequestHandler<EditSpecialityRequest, Shared.Result>
        {
            private readonly HRMDBContext _dbContext;
            private readonly IValidator<EditSpecialityRequest> _validator;

            public Handler(HRMDBContext dbContext, IValidator<EditSpecialityRequest> validator)
            {
                _dbContext = dbContext;
                _validator = validator;
            }
            public async Task<Shared.Result> Handle(EditSpecialityRequest request, CancellationToken cancellationToken)
            {
                var validationResponse = await _validator.ValidateAsync(request, cancellationToken);

                if (validationResponse.IsValid is false)
                {
                    return Shared.Result.Failure(Error.ValidationError(validationResponse));
                }
                var affectedrows = await _dbContext.Speciality.Where(x => x.Id == request.Id)
                    .ExecuteUpdateAsync(setters => setters
                    .SetProperty(c => c.specialityName, request.specialityName)
                    .SetProperty(c => c.categoryId, request.categoryId)
                    .SetProperty(c => c.updatedAt, DateTime.UtcNow)
                    );

                if (affectedrows == 0) return Shared.Result.Failure(Error.CreateNotFoundError("Staff Specialization Was Not Found"));

                return Shared.Result.Success();
            }
        }
    }
}

public class MapUpdateSpecilityEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("api/speciality/{Id}", async (ISender sender, Guid Id, AddSpecialtyRequest request) =>
        {
            var response = await sender.Send(
                new EditSpecialityRequest
                {
                    Id = Id,
                    categoryId = request.categoryId,
                    specialityName = request.specialityName
                }
                );

            if (response.IsFailure)
            {
                return Results.UnprocessableEntity(response.Error);
            }

            if (response.IsSuccess)
            {

                return Results.Ok();
            };

            return Results.BadRequest();

        }).WithTags("Setup-Staff-Speciality")
              .WithMetadata(new ProducesResponseTypeAttribute(typeof(Guid), StatusCodes.Status200OK))
              .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest));
    }
}