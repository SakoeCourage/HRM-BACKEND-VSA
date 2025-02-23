using Carter;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Entities;
using HRM_BACKEND_VSA.Extensions;
using HRM_BACKEND_VSA.Shared;
using HRM_BACKEND_VSA.Utilities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Contracts.UrlNavigation;
using static HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Department.GetDepartmentList;

namespace HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Department
{
    public static class GetDepartmentList
    {
        public class GetDepartmentListRequest : IFilterableSortableRoutePageParam, IRequest<Result<object>>
        {
            public string? search { get; set; }
            public string? sort { get; set; }
            public int? pageSize { get; set; }
            public int? pageNumber { get; set; }
        }

        public class Handler : IRequestHandler<GetDepartmentListRequest, Result<object>>
        {
            private readonly HRMDBContext _dBContext;
            public Handler(HRMDBContext dbContext)
            {
                _dBContext = dbContext;
            }
            public async Task<Result<object>> Handle(GetDepartmentListRequest request, CancellationToken cancellationToken)
            {
                var query = _dBContext.Department
                    .Include(dp => dp.headOfDepartment)
                    .Include(dp => dp.depHeadOfDepartment)
                    .AsQueryable();

                var queryBuilder = new QueryBuilder<Entities.Department>(query)
                        .WithSearch(request?.search, "departmentName")
                        .WithSort(request?.sort)
                        .Paginate(request?.pageNumber, request?.pageSize);

                var response = await queryBuilder.BuildAsync();

                return Shared.Result.Success(response);
            }
        }
    }
}

public class GetDepartmentListEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/department/all", async (ISender sender, [FromQuery] int? pageNumber, [FromQuery] int? pageSize, [FromQuery] string? search, [FromQuery] string? sort) =>
        {

            var response = await sender.Send(new GetDepartmentListRequest
            {
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
          .WithMetadata(new ProducesResponseTypeAttribute(typeof(Paginator.PaginatedData<Department>), StatusCodes.Status200OK))
          .WithGroupName(SwaggerDoc.SwaggerEndpointDefintions.Setup)
          .WithTags("Setup-Department");
    }

}