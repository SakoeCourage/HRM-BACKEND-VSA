using Carter;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Extensions;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Features.SMS_Setup.DeleteSMSTemplate;

namespace HRM_BACKEND_VSA.Features.SMS_Setup
{
    public static class DeleteSMSTemplate
    {
        public class DeleteSMSTemplateRequest : IRequest<Shared.Result>
        {
            public Guid Id { get; set; }
        }

        internal sealed class Hander : IRequestHandler<DeleteSMSTemplateRequest, Shared.Result>
        {
            protected readonly HRMDBContext _dbContext;
            public Hander(HRMDBContext dBContext)
            {
                _dbContext = dBContext;
            }
            public async Task<Shared.Result> Handle(DeleteSMSTemplateRequest request, CancellationToken cancellationToken)
            {
                var deletedRow = await _dbContext.SMSTemplate.Where(ent => ent.Id == request.Id && ent.readOnly == false)
                    .ExecuteDeleteAsync(cancellationToken);
                ;
                if (deletedRow == 0) return Shared.Result.Failure(Error.NotFound);

                return Shared.Result.Success();
            }
        }
    }
}

public class DeleteSMSTemplateEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/sms-template/{Id}", async (ISender sender, Guid Id) =>
        {
            var result = await sender.Send(new DeleteSMSTemplateRequest { Id = Id });

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
        .WithTags("Setup-SMS Template")
        .WithGroupName(SwaggerDoc.SwaggerEndpointDefintions.SMSService)
          ;
    }
}
