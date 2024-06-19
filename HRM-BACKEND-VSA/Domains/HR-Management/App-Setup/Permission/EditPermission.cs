using Carter;
using FluentValidation;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Entities;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Permission.EditPermission;
using static HRM_BACKEND_VSA.Features.Permission.CreatePermission;

namespace HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Permission
{
    public static class EditPermission
    {
        public class EditPermissionRequest : IRequest<Shared.Result<string>>
        {
            public string name { get; set; }
            public Guid id { get; set; }
        }

        public class Validator : AbstractValidator<EditPermissionRequest>
        {
            protected readonly IServiceScopeFactory _serviceScopeFactory;
            public Validator(IServiceScopeFactory scopeServiceFactory)
            {
                _serviceScopeFactory = scopeServiceFactory;

                RuleFor(x => x.name)
                    .NotEmpty()
                    .MustAsync(async (model, name, cancellationToken) =>
                    {
                        using (var scope = _serviceScopeFactory.CreateScope())
                        {
                            var dbContext = scope.ServiceProvider.GetRequiredService<HRMDBContext>();
                            bool exist = await dbContext
                            .Permission
                            .AnyAsync(e => e.name.ToLower() == name.Trim().ToLower() && e.Id != model.id);
                            return !exist;
                        }

                    })
                    .WithMessage("Permission Already Exist")
                    ;
            }
        }

        internal sealed class Handler(HRMDBContext dbContext, IValidator<EditPermissionRequest> validator) : IRequestHandler<EditPermissionRequest, Shared.Result<string>>
        {

            public async Task<Result<string>> Handle(EditPermissionRequest request, CancellationToken cancellationToken)
            {
                var validationResponse = await validator.ValidateAsync(request);
                if (validationResponse.IsValid is false)
                {
                    return Shared.Result.Failure<string>(Error.ValidationError(validationResponse));
                }

                var affectedRows = await dbContext.Permission.Where(p => p.Id == request.id).ExecuteUpdateAsync(setters =>
                    setters.SetProperty(c => c.updatedAt, DateTime.UtcNow)
                    .SetProperty(c => c.name, request.name)
                );

                if (affectedRows == 0)
                {
                    return Shared.Result.Failure<string>(Error.CreateNotFoundError("Permission Not Found"));
                }

                return Shared.Result.Success("Permission Name Updated Successfully");
            }
        }
    }
}

public class CreateEdittPermissionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("api/permission/{Id}", async (ISender sender, Guid Id, CreatePermissionDTO request) =>
        {
            var result = await sender.Send(new EditPermissionRequest
            {
                id = Id,
                name = request.name
            });

            if (result is null)
            {
                return Results.BadRequest(result?.Error);
            }
            if (result.IsSuccess)
            {
                return Results.Ok(result.Value);
            }

            return Results.BadRequest(result?.Error);

        }).WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest))
            .WithMetadata(new ProducesResponseTypeAttribute(typeof(Permission), StatusCodes.Status200OK))
            .WithTags("Setup-Permission")
            ;
    }
}
