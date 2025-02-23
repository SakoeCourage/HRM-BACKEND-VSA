using Carter;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Extensions;
using HRM_BACKEND_VSA.Model.SMS;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Features.SMS_Campaign.GetSMSCampaign;

namespace HRM_BACKEND_VSA.Features.SMS_Campaign
{
    public static class GetSMSCampaign
    {
        public class GetSMSCampaignRequestData : IRequest<Result<SMSCampaignHistory>>
        {
            public Guid Id { get; set; }
        }

        private sealed class Handler : IRequestHandler<GetSMSCampaignRequestData, Result<SMSCampaignHistory>>
        {
            private readonly HRMDBContext _dbContext;
            public Handler(HRMDBContext dBContext)
            {
                _dbContext = dBContext;
            }

            public async Task<Result<SMSCampaignHistory>> Handle(GetSMSCampaignRequestData request, CancellationToken cancellationToken)
            {
                var response = await _dbContext.SMSCampaignHistory
                .Include(q => q.smsTemplate)
                .Include(q => q.smsReceipients)
                .FirstOrDefaultAsync(q => q.Id == request.Id);

                if (response == null)
                {
                    return Shared.Result.Failure<SMSCampaignHistory>(Error.NotFound);
                }

                return Shared.Result.Success(response);
            }
        }
    }
}
public class MapGetSMSCampaignEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/sms-campaign/{id}",
        async (Guid id, ISender sender) =>
        {
            var response = await sender.Send(new GetSMSCampaignRequestData
            {
                Id = id
            });

            if (response is null)
            {
                return Results.NotFound(response?.Error);
            }

            if (response.IsSuccess)
            {
                return Results.Ok(response?.Value);
            }

            return Results.NotFound("Failed to SMS Campaign");
        })
            .WithMetadata(new ProducesResponseTypeAttribute(typeof(SMSCampaignHistory), StatusCodes.Status200OK))
            .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status401Unauthorized))
            .WithGroupName(SwaggerDoc.SwaggerEndpointDefintions.SMSService)
            .WithTags("SMS-Campaign");


    }
}