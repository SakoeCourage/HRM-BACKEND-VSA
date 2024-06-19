using Carter;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Features.Role;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRM_BACKEND_VSA.Features.Role
{
    public static class DeleteRole
    {

        public class DeleteRoleRequest : IRequest<Shared.Result>
        {
            public Guid Id { get; set; }
        }

        internal sealed class Hanlder : IRequestHandler<DeleteRoleRequest, Shared.Result>
        {
            HRMDBContext _dbContext;
            public Hanlder(HRMDBContext dbContext)
            {
                _dbContext = dbContext;
            }
            public async Task<Shared.Result> Handle(DeleteRoleRequest request, CancellationToken cancellationToken)
            {
                var role = await _dbContext.Role.FindAsync(request.Id, cancellationToken);

                if (role is null)
                {
                    return Shared.Result.Failure(Error.NotFound);
                }

                _dbContext.Role.Remove(role);
                try
                {
                    await _dbContext.SaveChangesAsync();
                    return Shared.Result.Success("Role Deleted Succesfully");
                }
                catch (DbUpdateException ex)
                {
                    return Shared.Result.Failure(Error.BadRequest(ex.Message));
                }
            }
        }
    }
}

public class MapDeleteUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/role/{Id}", async (Guid Id, ISender sender) =>
        {
            var response = await sender.Send(new DeleteRole.DeleteRoleRequest
            {
                Id = Id
            });

            if (response.IsFailure)
            {
                return Results.BadRequest(response.Error);
            }
            if (response.IsSuccess)
            {
                return Results.Ok("Role Deleted Successfully");
            }
            return Results.BadRequest(response.Error);

        }).WithTags("Setup-Role")
              .WithMetadata(new ProducesResponseTypeAttribute(StatusCodes.Status200OK))
              .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest))

        ;
    }
}