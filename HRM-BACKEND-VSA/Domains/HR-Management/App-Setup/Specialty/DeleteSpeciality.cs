using Carter;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Extensions;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Specialty.DeleteSpeciality;

namespace HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Specialty
{
    public static class DeleteSpeciality
    {
        public class DeleteSpecialityRequest : IRequest<Shared.Result>
        {
            public Guid Id { get; set; }
        }

        internal sealed class Hander : IRequestHandler<DeleteSpecialityRequest, Shared.Result>
        {
            protected readonly HRMDBContext _dbContext;
            public Hander(HRMDBContext dBContext)
            {
                _dbContext = dBContext;
            }
            public async Task<Shared.Result> Handle(DeleteSpecialityRequest request, CancellationToken cancellationToken)
            {
                var deletedRow = await _dbContext.Speciality.Where(ent => ent.Id == request.Id)
                    .ExecuteDeleteAsync(cancellationToken);
                ;
                if (deletedRow == 0) return Shared.Result.Failure(Error.NotFound);

                return Shared.Result.Success();
            }
        }
    }
}

public class DeletePlayerEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/speciality/{Id}", async (ISender sender, Guid Id) =>
        {
            var result = await sender.Send(new DeleteSpecialityRequest { Id = Id });

            if (result.IsFailure)
            {
                return Results.BadRequest(result?.Error);
            }
            if (result.IsSuccess)
            {
                return Results.NoContent();
            }

            return Results.BadRequest();
        })
        .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status422UnprocessableEntity))
        .WithTags("Setup-Staff-Speciality")
        .WithGroupName(SwaggerDoc.SwaggerEndpointDefintions.Setup)
          ;
    }
}
