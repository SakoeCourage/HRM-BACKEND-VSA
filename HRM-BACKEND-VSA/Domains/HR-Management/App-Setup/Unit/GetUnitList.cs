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
using static HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Unit.GetUnitList;

namespace HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Unit
{
    public class GetUnitList
    {
        public class GetUnitListRequest : IFilterableSortableRoutePageParam, IRequest<Result<object>>
        {
            public string? search { get; set; }
            public string? sort { get; set; }
            public int? pageSize { get; set; }
            public int? pageNumber { get; set; }
        }

        public class Handler : IRequestHandler<GetUnitListRequest, Result<object>>
        {
            private readonly HRMDBContext _dBContext;

            public Handler(HRMDBContext dbContext)
            {
                _dBContext = dbContext;
            }

            public async Task<Result<object>> Handle(GetUnitListRequest request, CancellationToken cancellationToken)
            {
                var query = _dBContext.Unit
                    .Include(un => un.unitHead)
                    .Include(un => un.directorate)
                    .Include(un => un.department)
                    .AsQueryable();

                var queryBuilder = new QueryBuilder<Entities.Unit>(query)
                    .WithSearch(request?.search, "unitname")
                    .WithSort(request?.sort)
                    .Paginate(request?.pageNumber, request?.pageSize);

                var response = await queryBuilder.BuildAsync((entry) =>
                    new SetupContract.UnitListResponseDTO
                    {
                        Id = entry.Id,
                        unitName = entry.unitName,
                        createdAt = entry.createdAt,
                        updatedAt = entry.updatedAt,
                        department = entry?.department is not null
                            ? new SetupContract.DepartmentListResponseDto
                            {
                                Id = entry.department.Id,
                                departmentName = entry.department.departmentName,
                                createdAt = entry.department.createdAt,
                                updatedAt = entry.department.updatedAt
                            }
                            : null,
                        directorate = entry?.directorate is not null
                            ? new SetupContract.DirectorateListResponseDto
                            {
                                Id = entry.directorate.Id,
                                directorateName = entry.directorate.directorateName,
                                createdAt = entry.directorate.createdAt,
                                updatedAt = entry.directorate.updatedAt
                            }
                            : null
                    }
                
                );

                return Shared.Result.Success(response);
            }
        }
    }
}

public class MapGetUnitListEnpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/unit/all", async (ISender sender, [FromQuery] int? pageNumber, [FromQuery] int? pageSize,
                [FromQuery] string? search, [FromQuery] string? sort) =>
            {
                var response = await sender.Send(new GetUnitListRequest
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
            .WithMetadata(new ProducesResponseTypeAttribute(
                typeof(Paginator.PaginatedData<SetupContract.UnitListResponseDTO>), StatusCodes.Status200OK))
            .WithTags("Setup-Unit")
            .WithGroupName(SwaggerDoc.SwaggerEndpointDefintions.Setup)
            ;
    }
}