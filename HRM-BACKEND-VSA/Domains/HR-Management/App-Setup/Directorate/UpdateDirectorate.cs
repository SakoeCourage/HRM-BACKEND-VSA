using Carter;
using FluentValidation;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Directorate;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Directorate.UpdateDirectorate;

namespace HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Directorate
{
    public static class UpdateDirectorate
    {
        public class UpdateDirectorateRequest : IRequest<Shared.Result>
        {
            public Guid Id { get; set; }
            public string directorateName { get; set; }
            public Guid? directorId { get; set; }
            public Guid? depDirectoryId { get; set; }

        }

        public class Validator : AbstractValidator<UpdateDirectorateRequest>
        {
            private readonly IServiceScopeFactory _scopeFactory;
            public Validator(IServiceScopeFactory scopefactory)
            {
                _scopeFactory = scopefactory;
                RuleFor(c => c.directorateName)
                    .NotEmpty()
                    .MustAsync(async (model, name, cancellationToken) =>
                    {
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var dbContext = scope.ServiceProvider.GetRequiredService<HRMDBContext>();
                            var exist = await dbContext.Directorate.AnyAsync(c => c.directorateName.ToLower() == name.ToLower() && c.Id != model.Id);
                            return !exist;
                        }
                    })
                    .WithMessage("Directorate Name Is Already Taken");
            }
        }

        internal sealed class Handler : IRequestHandler<UpdateDirectorateRequest, Shared.Result>
        {
            private readonly HRMDBContext _dbContext;
            private readonly IValidator<UpdateDirectorateRequest> _validator;

            public Handler(HRMDBContext dbContext, IValidator<UpdateDirectorateRequest> validator)
            {
                _dbContext = dbContext;
                _validator = validator;
            }
            public async Task<Shared.Result> Handle(UpdateDirectorateRequest request, CancellationToken cancellationToken)
            {

                var validationResult = await _validator.ValidateAsync(request, cancellationToken);

                if (validationResult.IsValid is false)
                {
                    return Shared.Result.Failure(Error.ValidationError(validationResult));
                }
                var affectedRows = await _dbContext.Directorate.Where(x => x.Id == request.Id).ExecuteUpdateAsync(setters =>
                setters.SetProperty(c => c.directorateName, request.directorateName)
                .SetProperty(c => c.updatedAt, DateTime.UtcNow)
                .SetProperty(c => c.directorId, request.directorId)
                .SetProperty(c => c.depDirectoryId, request.directorId)
                );

                if (affectedRows >= 1) return Shared.Result.Success();

                return Shared.Result.Failure(Error.CreateNotFoundError("Diretorate To Update Record Not Found"));
            }
        }

    }
}

public class MapUpdateDirectorateEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("api/directorate/{Id}", async (ISender sender, Guid Id, AddDirectorate.AddDirectorateRequest request) =>
        {
            var response = await sender.Send(
                new UpdateDirectorateRequest
                {
                    Id = Id,
                    directorateName = request.directorateName,
                    directorId = request.directorId,
                    depDirectoryId = request.depDirectoryId
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

        }).WithTags("Setup-Directorate")
              .WithMetadata(new ProducesResponseTypeAttribute(StatusCodes.Status204NoContent))
              .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest));
    }
}
