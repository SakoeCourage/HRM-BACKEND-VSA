using Carter;
using HRM_BACKEND_VSA.Contracts;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Extensions;
using HRM_BACKEND_VSA.Shared;
using HRM_BACKEND_VSA.Utilities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Contracts.UrlNavigation;
using static HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Specialty.GetSpecialityList;

namespace HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Specialty
{
    public static class GetSpecialityList
    {

        public class GetSpecialityListRequest : IFilterableSortableRoutePageParam, IRequest<Shared.Result<object>>
        {
            public string? search { get; set; }
            public string? sort { get; set; }
            public int? pageSize { get; set; }
            public int? pageNumber { get; set; }
        }

        public class Handler : IRequestHandler<GetSpecialityListRequest, Shared.Result<object>>
        {
            private readonly HRMDBContext _dBContext;
            public Handler(HRMDBContext dbContext)
            {
                _dBContext = dbContext;
            }
            public async Task<Shared.Result<object>> Handle(GetSpecialityListRequest request, CancellationToken cancellationToken)
            {
                var query = _dBContext.Speciality
                        .AsQueryable()
                        .Include(entry=>entry.category);

                var builder = new QueryBuilder<Entities.Speciality>(query)
                        .WithSearch(request?.search, "specialityName")
                        .WithSort(request?.sort)
                        .Paginate(request?.pageNumber, request?.pageSize);

                var response = await builder.BuildAsync((entry) => new SetupContract.SpecialityListResponseDto
                {
                    Id = entry.Id,
                    createdAt = entry.createdAt,
                    updatedAt = entry.updatedAt,
                    specialityName = entry.specialityName,
                    category = new SetupContract.CategoryListResponseDto
                    {
                        Id = entry.category.Id,
                        createdAt = entry.category.createdAt,
                        updatedAt = entry.category.updatedAt,
                        categoryName = entry.category.categoryName
                    }
                });

                return Shared.Result.Success(response);
            }
        }

    }
}

public class MapGetSpecialityListEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/speciality/all", async (ISender sender, [FromQuery] int? pageNumber, [FromQuery] int? pageSize, [FromQuery] string? search, [FromQuery] string? sort) =>
        {

            var response = await sender.Send(new GetSpecialityListRequest
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
          .WithMetadata(new ProducesResponseTypeAttribute(typeof(Paginator.PaginatedData<SetupContract.SpecialityListResponseDto>), StatusCodes.Status200OK))
          .WithGroupName(SwaggerDoc.SwaggerEndpointDefintions.Setup)
          .WithTags("Setup-Staff-Speciality");
    }

}