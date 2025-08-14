using Carter;
using HRM_BACKEND_VSA.Contracts;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Entities;
using HRM_BACKEND_VSA.Extensions;
using HRM_BACKEND_VSA.Shared;
using HRM_BACKEND_VSA.Utilities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Contracts.UrlNavigation;
using static HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Category.GetCategoryList;

namespace HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Category
{
    public class GetCategoryList
    {
        public class GetCategoryListRequest : IFilterableSortableRoutePageParam, IRequest<Result<object>>
        {
            public string? search { get; set; }
            public string? sort { get; set; }
            public int? pageSize { get; set; }
            public int? pageNumber { get; set; }
        }

        public class Handler : IRequestHandler<GetCategoryListRequest, Result<object>>
        {
            private readonly HRMDBContext _dBContext;
            public Handler(HRMDBContext dbContext)
            {
                _dBContext = dbContext;
            }
            public async Task<Result<object>> Handle(GetCategoryListRequest request, CancellationToken cancellationToken)
            {
                var query = _dBContext.Category.AsQueryable();

                var queryBuilder = new QueryBuilder<Entities.Category>(query)
                        .WithSearch(request?.search, "categoryName")
                        .WithSort(request?.sort)
                        .Paginate(request?.pageNumber, request?.pageSize);

                var response = await queryBuilder.BuildAsync((entry)=>new SetupContract.CategoryListResponseDto
                {
                    Id = entry.Id,
                    createdAt = entry.createdAt,
                    updatedAt = entry.updatedAt,
                    categoryName = entry.categoryName,
                });

                return Shared.Result.Success(response);
            }
        }

    }
}

public class GetCommunityListEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/category/all", async (ISender sender, [FromQuery] int? pageNumber, [FromQuery] int? pageSize, [FromQuery] string? search, [FromQuery] string? sort) =>
        {

            var response = await sender.Send(new GetCategoryListRequest
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
          .WithMetadata(new ProducesResponseTypeAttribute(typeof(Paginator.PaginatedData<SetupContract.CategoryListResponseDto>), StatusCodes.Status200OK))
          .WithGroupName(SwaggerDoc.SwaggerEndpointDefintions.Setup)
          .WithTags("Setup-Category");
    }

}
