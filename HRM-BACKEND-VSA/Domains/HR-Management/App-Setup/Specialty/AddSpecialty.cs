using Carter;
using FluentValidation;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using static HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Specialty.AddSpecialty;

namespace HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Specialty
{
    public class AddSpecialty
    {
        public class AddSpecialtyRequest : IRequest<Shared.Result<Guid>>
        {
            public Guid categoryId { get; set; }
            public string specialityName { get; set; } = String.Empty;
        }

        public class Validator : AbstractValidator<AddSpecialtyRequest>
        {
            private readonly IServiceScopeFactory _scopeFactory;
            public Validator(IServiceScopeFactory scopeFactory)
            {
                _scopeFactory = scopeFactory;
                RuleFor(c => c.specialityName).NotEmpty()
                    .MustAsync(async (name, cancellationToken) =>
                    {
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var dbContext = scope.ServiceProvider.GetService<HRMDBContext>();
                            var exist = await dbContext.Speciality.AnyAsync(s => s.specialityName == name, cancellationToken);
                            return !exist;
                        }
                    }).WithMessage("Speciality Name Already Exist")
                    ;
                RuleFor(c => c.categoryId).NotEmpty();

            }
        }

        public class Hanlder : IRequestHandler<AddSpecialtyRequest, Shared.Result<Guid>>
        {
            private readonly HRMDBContext _dbContext;
            private readonly IValidator<AddSpecialtyRequest> _validator;
            public Hanlder(HRMDBContext dbContext, IValidator<AddSpecialtyRequest> validator)
            {

                _dbContext = dbContext;
                _validator = validator;

            }
            public async Task<Shared.Result<Guid>> Handle(AddSpecialtyRequest request, CancellationToken cancellationToken)
            {
                var validationResponse = await _validator.ValidateAsync(request, cancellationToken);

                if (validationResponse.IsValid is false)
                {
                    return Shared.Result.Failure<Guid>(Error.ValidationError(validationResponse));
                }

                var newSpeciality = new Entities.Speciality
                {
                    specialityName = request.specialityName,
                    categoryId = request.categoryId,
                    createdAt = DateTime.UtcNow,
                    updatedAt = DateTime.UtcNow
                };

                _dbContext.Add(newSpeciality);

                try
                {
                    await _dbContext.SaveChangesAsync();
                }
                catch (DbException ex)
                {
                    return Shared.Result.Failure<Guid>(Error.BadRequest(ex.Message));
                }
                return Shared.Result.Success<Guid>(newSpeciality.Id);
            }
        }

    }
}

public class MappAddSpecialityRequest : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/speciality", async (ISender sender, AddSpecialtyRequest request) =>
        {
            var response = await sender.Send(request);
            if (response.IsFailure)
            {
                return Results.UnprocessableEntity(response.Error);
            }

            if (response.IsSuccess)
            {

                return Results.Ok(response.Value);
            }

            return Results.BadRequest();

        }).WithTags("Setup-Staff-Speciality")
              .WithMetadata(new ProducesResponseTypeAttribute(typeof(Guid), StatusCodes.Status200OK))
              .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest))

          ;
    }
}


