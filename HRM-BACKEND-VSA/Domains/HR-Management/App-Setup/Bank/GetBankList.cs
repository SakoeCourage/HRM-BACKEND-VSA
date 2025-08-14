using Carter;
using HRM_BACKEND_VSA.Contracts;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Extensions;
using HRM_BACKEND_VSA.Shared;
using HRM_BACKEND_VSA.Utilities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using static HRM_BACKEND_VSA.Contracts.UrlNavigation;
using static HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Bank.GetBankList;

namespace HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Bank
{
    public static class GetBankList
    {
        public class GetGetBankListRequest : IFilterableSortableRoutePageParam, IRequest<Result<object>>
        {
            public string? search { get; set; }
            public string? sort { get; set; }
            public int? pageSize { get; set; }
            public int? pageNumber { get; set; }
        }

        public class Handler : IRequestHandler<GetGetBankListRequest, Result<object>>
        {
            private readonly HRMDBContext _dBContext;
            public Handler(HRMDBContext dbContext)
            {
                _dBContext = dbContext;
            }
            public async Task<Result<object>> Handle(GetGetBankListRequest request, CancellationToken cancellationToken)
            {
                var query = _dBContext.Bank.AsQueryable();

                var queryBuilder = new QueryBuilder<Entities.HR_Manag.Bank>(query)
                        .WithSearch(request?.search, "bankName")
                        .WithSort(request?.sort)
                        .Paginate(request?.pageNumber, request?.pageSize);

                var response = await queryBuilder.BuildAsync((entry)=>new SetupContract.BankListResponseDto
                {
                    Id = entry.Id,
                    createdAt = entry.createdAt,
                    updatedAt = entry.updatedAt,
                    bankName = entry.bankName
                });

                return Shared.Result.Success(response);
            }
        }

    }
}

public class GetBankListEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/bank/all", async (ISender sender, [FromQuery] int? pageNumber, [FromQuery] int? pageSize, [FromQuery] string? search, [FromQuery] string? sort) =>
        {

            var response = await sender.Send(new GetGetBankListRequest
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
          .WithMetadata(new ProducesResponseTypeAttribute(typeof(Paginator.PaginatedData<SetupContract.BankListResponseDto>), StatusCodes.Status200OK))
          .WithTags("Setup-Bank")
          .WithGroupName(SwaggerDoc.SwaggerEndpointDefintions.Setup)
          ;
    }

}
