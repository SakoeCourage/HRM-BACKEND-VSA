using Carter;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Extensions;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Directorate.GetDirectorateById;

namespace HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Directorate
{
    public static class GetDirectorateById
    {
        public class GetDirectorateByIdRequest : IRequest<Shared.Result<Entities.Directorate>>
        {
            public Guid Id { get; set; }
        }

        internal sealed class Handler : IRequestHandler<GetDirectorateByIdRequest, Shared.Result<Entities.Directorate>>
        {
            private readonly HRMDBContext _dbContext;
            public Handler(HRMDBContext dbContext)
            {
                _dbContext = dbContext;
            }
            public async Task<Shared.Result<Entities.Directorate>> Handle(GetDirectorateByIdRequest request, CancellationToken cancellationToken)
            {
                var response = await _dbContext.Directorate
                    .Include(x => x.director)
                    .Include(x => x.depDirector)
                    .FirstOrDefaultAsync(x => x.Id == request.Id);

                if (response is null)
                {
                    return Shared.Result.Failure<Entities.Directorate>(Error.NotFound);
                }
                return Shared.Result.Success(response);
            }
        }
    }
}

public class GetDirectorateByIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {

        app.MapGet("api/directorate/{Id}",
        async (ISender sender, Guid id) =>
        {

            var response = await sender.Send(new GetDirectorateByIdRequest
            {
                Id = id
            });

            if (response.IsFailure)
            {
                return Results.BadRequest(response?.Error);
            }

            if (response.IsSuccess)
            {
                return Results.Ok(response.Value);
            }

            return Results.BadRequest(response?.Error);
        })
            .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest))
            .WithMetadata(new ProducesResponseTypeAttribute(typeof(HRM_BACKEND_VSA.Entities.Directorate), StatusCodes.Status200OK))
            .WithGroupName(SwaggerDoc.SwaggerEndpointDefintions.Setup)
            .WithTags("Setup-Directorate")
            ;
    }
}

