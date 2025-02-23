﻿using Carter;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Extensions;
using HRM_BACKEND_VSA.Model.SMS;
using HRM_BACKEND_VSA.Shared;
using HRM_BACKEND_VSA.Utilities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using static HRM_BACKEND_VSA.Contracts.UrlNavigation;
using static HRM_BACKEND_VSA.Features.SMS_Setup.GetSMSTemplateList;

namespace HRM_BACKEND_VSA.Features.SMS_Setup
{
    public static class GetSMSTemplateList
    {

        public class GetSMSTemplateListRequest : IFilterableSortableRoutePageParam, IRequest<Result<object>>
        {
            public string? search { get; set; }
            public string? sort { get; set; }
            public int? pageSize { get; set; }
            public int? pageNumber { get; set; }
        }

        public class Handler : IRequestHandler<GetSMSTemplateListRequest, Result<object>>
        {
            private readonly HRMDBContext _dBContext;
            public Handler(HRMDBContext dbContext)
            {
                _dBContext = dbContext;
            }
            public async Task<Result<object>> Handle(GetSMSTemplateListRequest request, CancellationToken cancellationToken)
            {
                var query = _dBContext.SMSTemplate.AsQueryable();

                var queryBuilder = new QueryBuilder<SMSTemplate>(query)
                        .WithSearch(request?.search, "name")
                        .WithSort(request?.sort)
                        .Paginate(request?.pageNumber, request?.pageSize);

                var response = await queryBuilder.BuildAsync();

                return Shared.Result.Success(response);
            }
        }
    }
}

public class MapGetSMSTemplateListEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/sms-template/all", async (ISender sender, [FromQuery] int? pageNumber, [FromQuery] int? pageSize, [FromQuery] string? search, [FromQuery] string? sort) =>
        {

            var response = await sender.Send(new GetSMSTemplateListRequest
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
          .WithMetadata(new ProducesResponseTypeAttribute(typeof(Paginator.PaginatedData<SMSTemplate>), StatusCodes.Status200OK))
          .WithTags("Setup-SMS Template")
          .WithGroupName(SwaggerDoc.SwaggerEndpointDefintions.SMSService)
          ;
    }

}
