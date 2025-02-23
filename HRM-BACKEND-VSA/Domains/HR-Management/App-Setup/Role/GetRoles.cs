using Carter;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Entities;
using HRM_BACKEND_VSA.Extensions;
using HRM_BACKEND_VSA.Features.Role;
using HRM_BACKEND_VSA.Shared;
using HRM_BACKEND_VSA.Utilities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Contracts.UrlNavigation;

namespace HRM_BACKEND_VSA.Features.Role
{
    public static class GetRoles
    {
        public class GetRoleRequest : IFilterableSortableRoutePageParam, IRequest<Shared.Result<object>>
        {
            public string? search { get; set; }
            public string? sort { get; set; }
            public int? pageSize { get; set; }
            public int? pageNumber { get; set; }
        }

        internal sealed class Handler : IRequestHandler<GetRoleRequest, Shared.Result<object>>
        {
            private readonly HRMDBContext _dbContext;
            public Handler(HRMDBContext dbContext)
            {
                _dbContext = dbContext;
            }
            public async Task<Shared.Result<object>> Handle(GetRoleRequest request, CancellationToken cancellationToken)
            {
                var initialQuery = _dbContext.Role.Include(r => r.permissions).AsQueryable();

                var queryBuilder = new QueryBuilder<Entities.Role>(initialQuery);
                Console.WriteLine("page size", request?.pageSize);
                queryBuilder
                    .WithSearch(request?.search, "name")
                    .WithSort(request?.sort)
                    .Paginate(request?.pageNumber, request?.pageSize);
                ;

                var result = await queryBuilder.BuildAsync();

                return Shared.Result.Success(result);
            }
        }
    }
}

public class GetRolesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {

        app.MapGet("api/role/all",
        async (ISender sender, [FromQuery] int? pageNumber, [FromQuery] int? pageSize, [FromQuery] string? search, [FromQuery] string? sort) =>
        {

            var response = await sender.Send(new GetRoles.GetRoleRequest
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
        })
            .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest))
            .WithMetadata(new ProducesResponseTypeAttribute(typeof(Paginator.PaginatedData<Role>), StatusCodes.Status200OK))
            .WithTags("Setup-Role")
            .WithGroupName(SwaggerDoc.SwaggerEndpointDefintions.Setup)
            ;
    }
}
