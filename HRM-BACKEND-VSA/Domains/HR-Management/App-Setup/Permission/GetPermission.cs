using Carter;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Entities;
using HRM_BACKEND_VSA.Extensions;
using HRM_BACKEND_VSA.Features.Permission;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HRM_BACKEND_VSA.Features.Permission
{
    public static class GetPermission
    {
        public class GetPermissionRequest : IRequest<Shared.Result<Entities.Permission?>>
        {
            public Guid Id { get; set; }
        }

        internal sealed class Hanlder : IRequestHandler<GetPermissionRequest, Shared.Result<Entities.Permission?>>
        {
            private readonly HRMDBContext _dbContext;
            public Hanlder(HRMDBContext dBContext)
            {
                _dbContext = dBContext;
            }
            public async Task<Shared.Result<Entities.Permission?>> Handle(GetPermissionRequest request, CancellationToken cancellationToken)
            {
                var permission = await _dbContext.Permission.FindAsync(request.Id);

                if (permission is null) return Shared.Result.Failure<Entities.Permission?>(Error.NotFound);

                return Shared.Result.Success<Entities.Permission?>(permission);

            }
        }
    }
}

public class CreateGetPermissionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/permission/{Id}", async (ISender sender, Guid Id) =>
        {
            var result = await sender.Send(new GetPermission.GetPermissionRequest
            {
                Id = Id
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
            .WithGroupName(SwaggerDoc.SwaggerEndpointDefintions.Setup)
            ;
    }
}
