using Carter;
using HRM_BACKEND_VSA.Contracts;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Extensions;
using HRM_BACKEND_VSA.Shared;
using HRM_BACKEND_VSA.Utilities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using static HRM_BACKEND_VSA.Contracts.UrlNavigation;
using static HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Directorate.GetDepartmentFromDirectorateId;

namespace HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Directorate
{
    public static class GetDepartmentFromDirectorateId
    {
        public class GetDepartmentFromDirectorateIdRequest : IRequest<Shared.Result<object>>, IFilterableSortableRoutePageParam
        {
            public Guid Id { get; set; }
            public string? search { get; set; }
            public string? sort { get; set; }
            public int? pageSize { get; set; }
            public int? pageNumber { get; set; }

        }

        internal sealed class Handler : IRequestHandler<GetDepartmentFromDirectorateIdRequest, Shared.Result<object>>
        {
            private readonly HRMDBContext _dbContext;

            public Handler(HRMDBContext dbContext)
            {
                _dbContext = dbContext;
            }
            public async Task<Result<object>> Handle(GetDepartmentFromDirectorateIdRequest request, CancellationToken cancellationToken)
            {
                var responseQuery = _dbContext.Department.Where(u => u.directorateId == request.Id).AsQueryable();

                var queryBuilder = new QueryBuilder<Entities.Department>(responseQuery)
                        .WithSearch(request?.search, "departmentName")
                        .WithSort(request?.sort)
                        .Paginate(request?.pageNumber, request?.pageSize);

                var response = await queryBuilder.BuildAsync((entry) =>  new SetupContract.DepartmentListResponseDto
                {
                    Id = entry.Id,
                    createdAt = entry.createdAt,
                    updatedAt = entry.updatedAt,
                    departmentName = entry.departmentName
                });
                
                return Shared.Result.Success(response);
            }
        }
    }
}


public class MapGetDepartmentFromDirectorateIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/directorate/{directorateId}/department/all", async (ISender sender, Guid directorateId, [FromQuery] int? pageNumber, [FromQuery] int? pageSize, [FromQuery] string? search, [FromQuery] string? sort) =>
        {

            var response = await sender.Send(new GetDepartmentFromDirectorateIdRequest
            {
                Id = directorateId,
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
        .WithGroupName(SwaggerDoc.SwaggerEndpointDefintions.Setup)
         .WithMetadata(new ProducesResponseTypeAttribute(typeof(Paginator.PaginatedData<SetupContract.DepartmentListResponseDto>), StatusCodes.Status200OK))
         .WithTags("Setup-Directorate");
    }
}