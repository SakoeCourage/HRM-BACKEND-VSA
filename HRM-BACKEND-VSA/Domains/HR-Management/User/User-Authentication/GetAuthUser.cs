using Carter;
using HRM_BACKEND_VSA.Providers;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static HRM_BACKEND_VSA.Domains.HR_Management.User.User_Authentication.GetAuthUser;

namespace HRM_BACKEND_VSA.Domains.HR_Management.User.User_Authentication
{
    public static class GetAuthUser
    {
        public class GetAuthUserRequest : IRequest<Shared.Result<Entities.User>>
        {

        }

        internal sealed class Handler : IRequestHandler<GetAuthUserRequest, Shared.Result<Entities.User>>
        {
            private readonly Authprovider _authProvider;
            public Handler(Authprovider authProvider)
            {
                _authProvider = authProvider;
            }
            public async Task<Result<Entities.User>> Handle(GetAuthUserRequest request, CancellationToken cancellationToken)
            {

                var authUser = await _authProvider.GetAuthUser();

                if (authUser is null) { return Shared.Result.Failure<Entities.User>(Error.CreateNotFoundError("Auth User Not Found")); }

                return Shared.Result.Success(authUser);
            }
        }
    }
}

public class MapGetAuhUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/auth/user",
            [Authorize(Policy = AuthorizationDecisionType.HRMUser)]
        async (ISender sender) =>
        {
            var response = await sender.Send(new GetAuthUserRequest { });

            if (response.IsSuccess)
            {
                return Results.Ok(response.Value);
            }

            return Results.Unauthorized();
        })
        .WithMetadata(new ProducesResponseTypeAttribute(typeof(HRM_BACKEND_VSA.Entities.User), StatusCodes.Status200OK))
        .WithMetadata(new ProducesResponseTypeAttribute(StatusCodes.Status401Unauthorized))
        .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status422UnprocessableEntity))
        .WithTags("Authentication-HRM-User")
            ;
    }
}
