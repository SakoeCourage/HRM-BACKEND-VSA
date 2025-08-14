using Carter;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Extensions;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Unit.RemoveUnit;

namespace HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Unit
{
    public static class RemoveUnit
    {

        public class DeleteUnitRequest : IRequest<Shared.Result>
        {
            public Guid Id { get; set; }
        }

        internal sealed class Handler : IRequestHandler<DeleteUnitRequest, Shared.Result>
        {
            private readonly HRMDBContext _dbContext;
            public Handler(HRMDBContext dbContext)
            {

                _dbContext = dbContext;

            }

            public async Task<Shared.Result> Handle(DeleteUnitRequest request, CancellationToken cancellationToken)
            {
                var affectedRows = await _dbContext
                    .Unit
                    .Where(s => s.Id == request.Id)
                    .ExecuteDeleteAsync();

                if (affectedRows == 0) return Shared.Result.Failure(Error.CreateNotFoundError("Unit Not Found"));
                return Shared.Result.Success();
            }
        }
    }
}
public class MapDeleteUnitEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/unit/{Id}", async (ISender sender, Guid Id) =>
        {
            var response = await sender.Send(new DeleteUnitRequest { Id = Id });

            if (response.IsFailure)
            {
                return Results.NotFound(response.Error);
            }

            return Results.NoContent();

        }).WithTags("Setup-Unit")
              .WithMetadata(new ProducesResponseTypeAttribute(StatusCodes.Status204NoContent))
              .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest))
              .WithGroupName(SwaggerDoc.SwaggerEndpointDefintions.Setup)
          ;
    }
}
