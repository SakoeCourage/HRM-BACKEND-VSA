using Carter;
using HRM_BACKEND_VSA.Entities.Staff;
using HRM_BACKEND_VSA.Providers;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static HRM_BACKEND_VSA.Domains.Staffs.Staff_Auth.GetAuthStaff;

namespace HRM_BACKEND_VSA.Domains.Staffs.Staff_Auth
{
    public static class GetAuthStaff
    {
        public class GetAuthStaffRequest : IRequest<Result<Entities.Staff.Staff>>
        {

        }

        internal sealed class Handler : IRequestHandler<GetAuthStaffRequest, Result<Entities.Staff.Staff>>
        {
            private readonly Authprovider _authProvider;
            public Handler(Authprovider authProvider)
            {
                _authProvider = authProvider;
            }
            public async Task<Result<Staff?>> Handle(GetAuthStaffRequest request, CancellationToken cancellationToken)
            {
                var staffData = await _authProvider.GetAuthStaff();

                if (staffData is null)
                {
                    return Shared.Result.Failure<Staff>(Error.CreateNotFoundError("Staff Not Found"));
                }
                return Shared.Result.Success(staffData);

            }
        }
    }
}

public class MapGetAuthStaffEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/staff/auth-staff",
        [Authorize(Policy = AuthorizationDecisionType.Staff)]
        async (ISender sender) =>
         {
             var result = await sender.Send(new GetAuthStaffRequest { });

             if (result.IsFailure)
             {
                 return Results.NotFound(result?.Error);
             }
             if (result.IsSuccess)
             {
                 return Results.Ok(result?.Value);
             }

             return Results.BadRequest();
         })
        .WithMetadata(new ProducesResponseTypeAttribute(typeof(Staff), StatusCodes.Status200OK))
        .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status422UnprocessableEntity))
        .WithTags("Authentication-Staff");
    }
}
