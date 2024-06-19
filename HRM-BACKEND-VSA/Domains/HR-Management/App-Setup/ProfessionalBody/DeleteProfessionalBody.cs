using Carter;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.ProfessionalBody.DeleteProfessionalBody;

namespace HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.ProfessionalBody
{
    public static class DeleteProfessionalBody
    {
        public class DeleteProfessionalBodyRequest : IRequest<Shared.Result>
        {
            public Guid Id { get; set; }
        }
        public class Handler : IRequestHandler<DeleteProfessionalBodyRequest, Shared.Result>
        {
            private readonly HRMDBContext _dbContext;
            public Handler(HRMDBContext dbContext)
            {

                _dbContext = dbContext;

            }

            public async Task<Shared.Result> Handle(DeleteProfessionalBodyRequest request, CancellationToken cancellationToken)
            {
                var affectedRows = await _dbContext
                    .ProfessionalBody
                    .Where(s => s.Id == request.Id)
                    .ExecuteDeleteAsync();

                if (affectedRows == 0) return Shared.Result.Failure(Error.NotFound);
                return Shared.Result.Success();
            }
        }
    }
}

public class MapDeleteProfessionalBodyEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/professional-body/{Id}", async (ISender sender, Guid Id) =>
        {
            var response = await sender.Send(new DeleteProfessionalBodyRequest { Id = Id });

            if (response.IsFailure)
            {
                return Results.NotFound(response.Error);
            }

            return Results.NoContent();

        }).WithTags("Setup-ProfessionalBody")
              .WithMetadata(new ProducesResponseTypeAttribute(StatusCodes.Status204NoContent))
              .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest))

          ;
    }
}
