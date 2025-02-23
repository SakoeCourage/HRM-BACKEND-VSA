using Carter;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Extensions;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Domains.HR_Management.User.DeleteUser;

namespace HRM_BACKEND_VSA.Domains.HR_Management.User
{
    public static class DeleteUser
    {
        public class DeleteUserRequest : IRequest<Shared.Result>
        {
            public Guid id { get; set; }
        }


        public class Handler : IRequestHandler<DeleteUserRequest, Shared.Result>
        {
            private readonly HRMDBContext _dbContext;

            public Handler(HRMDBContext dbContext)
            {
                _dbContext = dbContext;
            }
            public async Task<Shared.Result> Handle(DeleteUserRequest request, CancellationToken cancellationToken)
            {
                var affectedRows = await _dbContext.User.Where(u => u.Id == request.id).ExecuteDeleteAsync(cancellationToken);

                if (affectedRows == 0) return Shared.Result.Failure(Error.CreateNotFoundError("Requested User Not Found"));

                return Shared.Result.Success();
            }
        }
    }
}

public class MapDeleteUserEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/user/{id}", async (ISender sender, Guid id) =>
        {
            var response = await sender.Send(new DeleteUserRequest
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
        .WithGroupName(SwaggerDoc.SwaggerEndpointDefintions.UserManagement)
            ;
    }
}
