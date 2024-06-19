using Carter;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Domains.Staffs.Services;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Domains.HR_Management.Staff_Request.GetStaffRequestData;

namespace HRM_BACKEND_VSA.Domains.HR_Management.Staff_Request
{
    public static class GetStaffRequestData
    {
        public class StaffRequestData : IRequest<Shared.Result<object>>
        {
            public Guid Id { get; set; }

        }


        internal sealed class Handler : IRequestHandler<StaffRequestData, Shared.Result<object>>
        {
            private readonly HRMDBContext _dbContext;
            private readonly HRMStaffDBContext _staffdbContext;
            private readonly RequestService _requestService;


            public Handler(HRMDBContext dbContext, HRMStaffDBContext staffdbContext, RequestService requestService)
            {
                _dbContext = dbContext;
                _staffdbContext = staffdbContext;
                _requestService = requestService;
            }

            public async Task<Result<object>> Handle(StaffRequestData request, CancellationToken cancellationToken)
            {

                var requestRecord = await _dbContext.StaffRequest.FirstOrDefaultAsync(x => x.Id == request.Id);

                if (requestRecord is null)
                {
                    return Shared.Result.Failure<object>(Error.CreateNotFoundError("Staff Request Not Found"));
                }

                var service = _requestService.getRequestService(requestRecord.requestType);

                Console.WriteLine($"Request Service Found AS {service}");

                if (service is null)
                {
                    return Shared.Result.Failure<object>(Error.CreateNotFoundError("Staff Request Type Not Found"));
                }

                var response = await service.GetStaffRequestData(requestRecord.RequestDetailPolymorphicId);

                if (response is null)
                {
                    return Shared.Result.Failure<object>(Error.CreateNotFoundError("Staff Request Type Not Found"));
                }

                if (response is not null)
                {
                    return Shared.Result.Success<object>(response);
                }

                return Shared.Result.Failure<object>(Error.CreateNotFoundError("Staff Request Type Not Found"));

            }
        }
    }
}

public class MapGetStaffRequestEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/staff-request/{id}", async (ISender sender, Guid id) =>
        {
            var response = await sender.Send(new StaffRequestData
            {
                Id = id
            });

            if (response.IsFailure)
            {
                return Results.BadRequest(response.Error);
            }

            if (response.IsSuccess)
            {
                return Results.Ok(response.Value);
            }
            return Results.BadRequest();
        }).WithTags("Staff-Request");
    }
}
