using Carter;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Entities;
using HRM_BACKEND_VSA.Extensions;
using HRM_BACKEND_VSA.Features.Role;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRM_BACKEND_VSA.Features.Role
{
    public static class GetRole
    {

        public class GetReleRequest : IRequest<Shared.Result<Entities.Role>>
        {
            public Guid Id { get; set; }
        }


        internal sealed class Handler : IRequestHandler<GetReleRequest, Shared.Result<Entities.Role>>
        {
            private readonly HRMDBContext _dbContext;
            public Handler(HRMDBContext dbContext)
            {
                _dbContext = dbContext;
            }
            public async Task<Shared.Result<Entities.Role>> Handle(GetReleRequest request, CancellationToken cancellationToken)
            {
                var role = await _dbContext.Role.Include(x => x.permissions).FirstOrDefaultAsync(x => x.Id == request.Id);

                if (role is null)
                {
                    return Shared.Result.Failure<Entities.Role>(Error.NotFound);
                }
                return Shared.Result.Success(role);
            }
        }
    }
}


public class GetRoleEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {

        app.MapGet("api/role/{Id}",
        async (ISender sender, Guid id) =>
        {

            var response = await sender.Send(new GetRole.GetReleRequest
            {
                Id = id
            });

            if (response.IsFailure)
            {
                return Results.BadRequest(response?.Error);
            }

            if (response.IsSuccess)
            {
                return Results.Ok(response.Value);
            }

            return Results.BadRequest(response?.Error);
        })
            .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest))
            .WithMetadata(new ProducesResponseTypeAttribute(typeof(Role), StatusCodes.Status200OK))
            .WithTags("Setup-Role")
            .WithGroupName(SwaggerDoc.SwaggerEndpointDefintions.Setup)
            ;
    }
}