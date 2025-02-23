using Carter;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Extensions;
using HRM_BACKEND_VSA.Shared;
using HRM_BACKEND_VSA.Utilities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using static HRM_BACKEND_VSA.Contracts.UrlNavigation;
using static HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Department.GetUnitFromDepId;

namespace HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Department
{
    public static class GetUnitFromDepId
    {
        public class GetUnitFromDepIdRequest : IRequest<Shared.Result<object>>, IFilterableSortableRoutePageParam
        {
            public Guid Id { get; set; }
            public string? search { get; set; }
            public string? sort { get; set; }
            public int? pageSize { get; set; }
            public int? pageNumber { get; set; }

        }


        internal sealed class Handler : IRequestHandler<GetUnitFromDepIdRequest, Shared.Result<object>>
        {
            private readonly HRMDBContext _dbContext;

            public Handler(HRMDBContext dbContext)
            {
                _dbContext = dbContext;
            }
            public async Task<Result<object>> Handle(GetUnitFromDepIdRequest request, CancellationToken cancellationToken)
            {
                var responseQuery = _dbContext.Unit.Where(u => u.departmentId == request.Id).AsQueryable();

                var queryBuilder = new QueryBuilder<Entities.Unit>(responseQuery)
                        .WithSearch(request?.search, "unitName")
                        .WithSort(request?.sort)
                        .Paginate(request?.pageNumber, request?.pageSize);

                var response = await queryBuilder.BuildAsync();

                return Shared.Result.Success(response);
            }
        }
    }
}


public class MapGetUnitFromDepIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/department/{departmentId}/unit/all", async (ISender sender, Guid departmentId, [FromQuery] int? pageNumber, [FromQuery] int? pageSize, [FromQuery] string? search, [FromQuery] string? sort) =>
        {

            var response = await sender.Send(new GetUnitFromDepIdRequest
            {
                Id = departmentId,
                pageSize = pageSize,
                pageNumber = pageNumber,
                search = search,
                sort = sort
            });

            if (response is null)
            {
                return Results.BadRequest("Empty Result");
            }

            if (response.IsSuccess)
            {
                return Results.Ok(response.Value);
            }
            return Results.BadRequest("Empty Result");
        }).WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest))
         .WithMetadata(new ProducesResponseTypeAttribute(typeof(Paginator.PaginatedData<HRM_BACKEND_VSA.Entities.Unit>), StatusCodes.Status200OK))
         .WithGroupName(SwaggerDoc.SwaggerEndpointDefintions.Setup)
         .WithTags("Setup-Department");
    }
}
