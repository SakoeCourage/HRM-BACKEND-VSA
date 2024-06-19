using Carter;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Directorate.DeleteDirectorate;

namespace HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Directorate
{
    public class DeleteDirectorate
    {
        public class DeleteDirectorateRequest : IRequest<Shared.Result>
        {
            public Guid Id { get; set; }
        }

        internal sealed class Handler : IRequestHandler<DeleteDirectorateRequest, Shared.Result>
        {
            private readonly HRMDBContext _dbContext;
            public Handler(HRMDBContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<Shared.Result> Handle(DeleteDirectorateRequest request, CancellationToken cancellationToken)
            {
                var affectedRows = await _dbContext
                    .Directorate
                    .Where(s => s.Id == request.Id)
                    .ExecuteDeleteAsync();

                if (affectedRows == 0) return Shared.Result.Failure(Error.CreateNotFoundError("Department Not Found"));
                return Shared.Result.Success();
            }
        }
    }
}

public class MapDeleteDirectorateEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/directorate/{Id}", async (ISender sender, Guid Id) =>
        {
            var response = await sender.Send(new DeleteDirectorateRequest { Id = Id });

            if (response.IsFailure)
            {
                return Results.NotFound(response.Error);
            }

            return Results.NoContent();

        }).WithTags("Setup-Directorate")
              .WithMetadata(new ProducesResponseTypeAttribute(StatusCodes.Status204NoContent))
              .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest))
          ;
    }
}
