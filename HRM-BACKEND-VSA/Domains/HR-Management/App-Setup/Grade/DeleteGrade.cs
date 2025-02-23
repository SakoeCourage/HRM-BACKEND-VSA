using Carter;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Extensions;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Grade.DeleteGrade;

namespace HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Grade
{
    public static class DeleteGrade
    {

        public class DeleteGradeRequest : IRequest<Shared.Result>
        {
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<DeleteGradeRequest, Shared.Result>
        {
            private readonly HRMDBContext _dbContext;
            public Handler(HRMDBContext dbContext)
            {

                _dbContext = dbContext;

            }

            public async Task<Shared.Result> Handle(DeleteGradeRequest request, CancellationToken cancellationToken)
            {
                var affectedRows = await _dbContext
                    .Grade
                    .Where(s => s.Id == request.Id)
                    .ExecuteDeleteAsync();

                if (affectedRows == 0) return Shared.Result.Failure(Error.CreateNotFoundError("Requested Grade Was Not Found"));
                return Shared.Result.Success();
            }
        }

    }
}


public class MapDeleteGradeEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/grade/{Id}", async (ISender sender, Guid Id) =>
        {
            var response = await sender.Send(new DeleteGradeRequest { Id = Id });

            if (response.IsFailure)
            {
                return Results.NotFound(response.Error);
            }

            if (response.IsSuccess)
            {

                return Results.NoContent();
            }
            return Results.BadRequest();
        }).WithTags("Setup-Grade")
              .WithMetadata(new ProducesResponseTypeAttribute(StatusCodes.Status204NoContent))
              .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest))
              .WithGroupName(SwaggerDoc.SwaggerEndpointDefintions.Setup)

          ;
    }
}