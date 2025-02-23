using Carter;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Domains.Staffs.Services;
using HRM_BACKEND_VSA.Extensions;
using HRM_BACKEND_VSA.Providers;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Domains.HR_Management.Staff_Request.RejectStaffRequest;

namespace HRM_BACKEND_VSA.Domains.HR_Management.Staff_Request
{
    public static class RejectStaffRequest
    {
        public class RejectStaffRequestData : IRequest<Shared.Result<object>>
        {
            public Guid Id { get; set; }
            public string? description { get; set; }
        }

        internal sealed class Handler : IRequestHandler<RejectStaffRequestData, Shared.Result<object>>
        {
            private readonly HRMDBContext _dbContext;
            private readonly HRMStaffDBContext _staffdbContext;
            private readonly RequestService _requestService;
            private readonly Authprovider _authProvider;

            public Handler(HRMDBContext dbContext, HRMStaffDBContext staffdbContext, RequestService requestService, Authprovider authProvider)
            {
                _dbContext = dbContext;
                _staffdbContext = staffdbContext;
                _requestService = requestService;
                _authProvider = authProvider;
            }

            public async Task<Result<object>> Handle(RejectStaffRequestData request, CancellationToken cancellationToken)
            {

                var authUser = await _authProvider.GetAuthUser();

                if (authUser is null)
                {
                    return Shared.Result.Failure<object>(Error.CreateNotFoundError("Failed To Authorized User"));
                }


                var requestRecord = await _dbContext.StaffRequest.FirstOrDefaultAsync(sr => sr.Id == request.Id);

                if (requestRecord is null)
                {
                    return Shared.Result.Failure<object>(Error.CreateNotFoundError("Staff Request Record Not Found"));
                }

                var service = _requestService.getRequestService(requestRecord.requestType);

                if (service is null)
                {
                    return Shared.Result.Failure<object>(Error.CreateNotFoundError("Staff Request Type Not Found"));
                }

                try
                {
                    await service.OnRequestRejected(requestRecord.RequestDetailPolymorphicId, request.description, requestRecord);
                    return Shared.Result.Success<object>("Staff Request Rejected");
                }
                catch (Exception ex)
                {
                    return Shared.Result.Failure<object>(Error.BadRequest(ex.Message));
                }

            }
        }
    }
}


public class MapRejectStaffRequestEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/staff-request/reject",
         [Authorize(Policy = AuthorizationDecisionType.HRMUser)]
        async (ISender sender, RejectStaffRequestData request) =>
         {
             var response = await sender.Send(request);

             if (response.IsFailure)
             {
                 return Results.BadRequest(response.Error);
             }

             if (response.IsSuccess)
             {
                 return Results.Ok(response.Value);
             }
             return Results.BadRequest();
         }).WithTags("Staff-Request")
            .WithGroupName(SwaggerDoc.SwaggerEndpointDefintions.StaffRequestHandler)
            ;
    }
}