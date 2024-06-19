using Carter;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Entities;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Grade.GetGrade;

namespace HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Grade
{
    public static class GetGrade
    {
        public class getGradeRequest : IRequest<Shared.Result<Entities.Grade>>
        {
            public Guid id { get; set; }
        }

        internal sealed class Handler(HRMDBContext dbContext) : IRequestHandler<getGradeRequest, Shared.Result<Entities.Grade>>
        {
            public async Task<Result<Entities.Grade>> Handle(getGradeRequest request, CancellationToken cancellationToken)
            {
                var response = await dbContext.Grade
                    .Include(g => g.steps)
                    .FirstOrDefaultAsync(x => x.Id == request.id);

                if (response == null)
                {
                    return Shared.Result.Failure<Entities.Grade>(Error.CreateNotFoundError("Grade Not Found"));
                }

                return Shared.Result.Success(response);
            }
        }
    }
}

public class MapGetGradeEnpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/grade/{id}", async (ISender sender, Guid id) =>
        {
            var response = await sender.Send(
                    new getGradeRequest
                    {
                        id = id
                    }
                );

            if (response.IsFailure)
            {
                return Results.UnprocessableEntity(response.Error);
            }

            if (response.IsSuccess)
            {
                return Results.Ok(response.Value);
            };

            return Results.BadRequest();

        }).WithTags("Setup-Grade")
           .WithMetadata(new ProducesResponseTypeAttribute(typeof(Grade), StatusCodes.Status200OK))
           .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest));
    }
}
