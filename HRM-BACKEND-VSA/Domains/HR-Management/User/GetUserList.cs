﻿using Carter;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Extensions;
using HRM_BACKEND_VSA.Shared;
using HRM_BACKEND_VSA.Utilities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Contracts.UrlNavigation;
using static HRM_BACKEND_VSA.Domains.HR_Management.User.GetUserList;

namespace HRM_BACKEND_VSA.Domains.HR_Management.User
{
    public static class GetUserList
    {
        public class GetUserListRequest : IFilterableSortableRoutePageParam, IRequest<Shared.Result<object>>
        {
            public string? search { get; set; }
            public string? sort { get; set; }
            public int? pageSize { get; set; }
            public int? pageNumber { get; set; }
        }
        public class Handler : IRequestHandler<GetUserListRequest, Shared.Result<object>>
        {
            private readonly HRMDBContext _dBContext;
            public Handler(HRMDBContext dbContext)
            {
                _dBContext = dbContext;
            }
            public async Task<Shared.Result<object>> Handle(GetUserListRequest request, CancellationToken cancellationToken)
            {
                var query = _dBContext.User.Include(u => u.staff).Include(u => u.role).AsQueryable().IgnoreAutoIncludes();

                var queryBuilder = new QueryBuilder<Entities.User>(query)
                        .WithSearch(request?.search, "email")
                        .WithSort(request?.sort)
                        .Paginate(request?.pageNumber, request?.pageSize);

                var response = await queryBuilder.BuildAsync();

                return Shared.Result.Success(response);
            }
        }
    }
}
public class MapGetClubListEnpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/user/all", async (ISender sender, [FromQuery] int? pageNumber, [FromQuery] int? pageSize, [FromQuery] string? search, [FromQuery] string? sort) =>
        {
            var response = await sender.Send(new GetUserListRequest
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
          .WithMetadata(new ProducesResponseTypeAttribute(typeof(Paginator.PaginatedData<HRM_BACKEND_VSA.Entities.User>), StatusCodes.Status200OK))
          .WithTags("Manage-User")
          .WithGroupName(SwaggerDoc.SwaggerEndpointDefintions.UserManagement)
          ;
    }
}
