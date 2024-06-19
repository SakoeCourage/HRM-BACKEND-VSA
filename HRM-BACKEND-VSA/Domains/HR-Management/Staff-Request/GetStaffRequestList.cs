using Carter;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Domains.Staffs.Services;
using HRM_BACKEND_VSA.Shared;
using HRM_BACKEND_VSA.Utilities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Contracts.UrlNavigation;
using static HRM_BACKEND_VSA.Domains.HR_Management.Staff_Request.GetStaffRequestList;

namespace HRM_BACKEND_VSA.Domains.HR_Management.Staff_Request
{
    public static class GetStaffRequestList
    {
        public class GetStaffRequestListListRequest : IFilterableSortableRoutePageParam, IRequest<Result<object>>
        {
            public string? search { get; set; }
            public string? sort { get; set; }
            public int? pageSize { get; set; }
            public int? pageNumber { get; set; }
            public string? filter { get; set; }
        }

        public class Handler : IRequestHandler<GetStaffRequestListListRequest, Result<object>>
        {
            private readonly HRMDBContext _dBContext;
            private readonly HRMStaffDBContext _staffdbContext;
            RequestService _requestService;
            public Handler(HRMDBContext dbContext, RequestService requestService)
            {
                _dBContext = dbContext;
                _requestService = requestService;
            }
            public async Task<Result<object>> Handle(GetStaffRequestListListRequest request, CancellationToken cancellationToken)
            {
                var query = _dBContext.StaffRequest.Include(x => x.requestFromStaff).Include(r => r.requestAssignedStaff).AsQueryable();

                if (request.filter is not null)
                {
                    var service = _requestService.getRequestService(request.filter);
                    if (service is not null)
                    {
                        query = query.Where(x => x.requestType == request.filter).AsQueryable();
                    }
                }

                var queryBuilder = new QueryBuilder<Entities.HR_Manag.StaffRequest>(query)
                        .WithSearch(request?.search, "requestFromStaffId")
                        .WithSort(request?.sort)
                        .Paginate(request?.pageNumber, request?.pageSize);

                var response = await queryBuilder.BuildAsync();

                return Shared.Result.Success(response);
            }
        }
    }

    public class GetStaffRequestListEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/staff-request/all", async (ISender sender, [FromQuery] int? pageNumber, [FromQuery] int? pageSize, [FromQuery] string? search, [FromQuery] string? sort, [FromQuery] string? filter) =>
            {

                var response = await sender.Send(new GetStaffRequestListListRequest
                {
                    pageSize = pageSize,
                    pageNumber = pageNumber,
                    search = search,
                    sort = sort,
                    filter = filter
                });

                if (response is null)
                {
                    return Results.BadRequest("Empty Result");
                }

                if (response.IsSuccess)
                {
                    return Results.Ok(response.Value);
                }


                return Results.BadRequest();
            }).WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest))
              .WithMetadata(new ProducesResponseTypeAttribute(typeof(Paginator.PaginatedData<HRM_BACKEND_VSA.Entities.HR_Manag.StaffRequest>), StatusCodes.Status200OK))
              .WithTags("Staff-Request");
        }

    }
}
