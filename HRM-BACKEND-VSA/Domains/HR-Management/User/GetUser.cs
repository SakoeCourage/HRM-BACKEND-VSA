using Carter;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Extensions;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Domains.HR_Management.User.GetUser;

namespace HRM_BACKEND_VSA.Domains.HR_Management.User
{
    public static class GetUser
    {
        public class GetUserRequest : IRequest<Shared.Result<Entities.User>>
        {
            public Guid id { get; set; }
        }

        public class Handler : IRequestHandler<GetUserRequest, Shared.Result<Entities.User>>
        {
            private readonly HRMDBContext _dbContext;
            public Handler(HRMDBContext dbContext)
            {
                _dbContext = dbContext;
            }
            public async Task<Shared.Result<Entities.User>> Handle(GetUserRequest request, CancellationToken cancellationToken)
            {
                var user = await _dbContext.User.FirstOrDefaultAsync(u => u.Id == request.id);
                if (user == null) { return Shared.Result.Failure<Entities.User>(Error.CreateNotFoundError("User Not Found")); }

                return Shared.Result.Success<Entities.User>(user);
            }
        }
    }
}

public class MapGetUserEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/user/{id}", async (ISender sender, Guid id) =>
        {
            var response = await sender.Send(new GetUserRequest
            {
                id = id
            });

            if (response.IsSuccess)
            {
                return Results.NoContent();
            }
            if (response.IsFailure)
            {
                return Results.UnprocessableEntity(response.Error);
            }

            return Results.BadRequest();
        }).WithTags("Manage-User")
        .WithMetadata(new ProducesResponseTypeAttribute(typeof(HRM_BACKEND_VSA.Entities.User), StatusCodes.Status200OK))
        .WithGroupName(SwaggerDoc.SwaggerEndpointDefintions.UserManagement)
        ;
    }
}
