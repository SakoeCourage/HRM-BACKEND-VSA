using Carter;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Extensions;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Bank.DeleteBank;

namespace HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Bank
{
    public static class DeleteBank
    {
        public class DeleteBankRequest : IRequest<Shared.Result>
        {
            public Guid Id { get; set; }
        }

        internal sealed class Handler : IRequestHandler<DeleteBankRequest, Shared.Result>
        {
            private readonly HRMDBContext _dbContext;
            public Handler(HRMDBContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<Shared.Result> Handle(DeleteBankRequest request, CancellationToken cancellationToken)
            {
                var affectedRows = await _dbContext
                    .Bank
                    .Where(s => s.Id == request.Id)
                    .ExecuteDeleteAsync();

                if (affectedRows == 0) return Shared.Result.Failure(Error.CreateNotFoundError("Bank Not Found"));
                return Shared.Result.Success();
            }
        }

    }
}

public class DeleteBankEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/bank/{Id}", async (ISender sender, Guid Id) =>
        {
            var response = await sender.Send(new DeleteBankRequest { Id = Id });

            if (response.IsFailure)
            {
                return Results.NotFound(response.Error);
            }

            return Results.NoContent();

        }).WithTags("Setup-Bank")
              .WithMetadata(new ProducesResponseTypeAttribute(StatusCodes.Status204NoContent))
              .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest))
              .WithGroupName(SwaggerDoc.SwaggerEndpointDefintions.Setup)
          ;
    }
}
