using Carter;
using FluentValidation;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Domains.HR_Management.SMS_Campaign;
using HRM_BACKEND_VSA.Serivices.Mail_Service;
using HRM_BACKEND_VSA.Services.SMS_Service;
using HRM_BACKEND_VSA.Shared;
using HRM_BACKEND_VSA.SMS_Campaign;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using static HRM_BACKEND_VSA.Contracts.SMSContracts;
using static HRM_BACKEND_VSA.Features.SMS_Campaign.CreateSMSCampaign;

namespace HRM_BACKEND_VSA.Features.SMS_Campaign
{
    public static class CreateSMSCampaign
    {
        public class CreateSMSCampaignRequest : IRequest<Result<string>>
        {
            [Required]
            public string campaingName { get; set; }
            [Required]
            public Guid? smsTemplateId { get; set; }
            public IFormFile? templateFile { get; set; }
            public string? message { get; set; }
            public Guid[]? staffIds { get; set; }
            public string? directorateId { get; set; }
            public string? departmentId { get; set; }
            public string? unitId { get; set; }
            public string? frequency { get; set; }
        }

        public class Validator : AbstractValidator<CreateSMSCampaignRequest>
        {
            private readonly IServiceScopeFactory _scopeFactory;
            public Validator(IServiceScopeFactory scopeFactory)
            {
                _scopeFactory = scopeFactory;
                RuleFor(x => x.campaingName)
                 .NotEmpty()
                 .MustAsync(async (name, cancellationToken) =>
                 {
                     using (var scope = _scopeFactory.CreateScope())
                     {
                         var dbContext = scope.ServiceProvider.GetRequiredService<HRMDBContext>();
                         bool exist = await dbContext
                         .SMSCampaignHistory
                         .AnyAsync(e => e.campaignName.ToLower() == name.Trim().ToLower());
                         return !exist;
                     }

                 })
                 .WithMessage("Campaign Name Already Exist")
                 ;
                RuleFor(c => c.message)
                     .Must((model, message) =>
                     {
                         return (model.smsTemplateId == null && message == String.Empty) ? false : true;
                     })
                     .WithMessage("Message Is Required");
            }
        }

        public class Handler : IRequestHandler<CreateSMSCampaignRequest, Result<string>>
        {
            private readonly HRMDBContext _dbContext;
            private readonly IValidator<CreateSMSCampaignRequest> _validator;
            private readonly SMSService _smsService;
            private readonly HRMStaffDBContext _staffDbContext;
            private readonly MailService _mailService;
            public Handler(HRMDBContext dbContext, IValidator<CreateSMSCampaignRequest> validator, SMSService smsService, HRMStaffDBContext staffDbContext, MailService mailService)
            {
                _dbContext = dbContext;
                _validator = validator;
                _smsService = smsService;
                _staffDbContext = staffDbContext;
                _mailService = mailService;
            }

            public async Task<Result<string>> Handle(CreateSMSCampaignRequest request, CancellationToken cancellationToken)
            {
                var validationResponse = await _validator.ValidateAsync(request, cancellationToken);

                if (validationResponse.IsValid is false)
                {
                    return Shared.Result.Failure<string>(Error.ValidationError(validationResponse));
                }

                // Without File Request
                var NoFileRequest = new NewNonFileTemplateSMSDTO
                {
                    smsTemplateId = request.smsTemplateId,
                    campaingName = request.campaingName,
                    message = request.message,
                    frequency = request.frequency

                };

                // With File Request
                var filtemplateDto = new NewFileTemplateSMSDTO
                {
                    campaingName = request.campaingName,
                    smsTemplateId = request.smsTemplateId,
                    templateFile = request.templateFile,
                    message = request?.message,
                    frequency = request?.frequency
                };

                try
                {
                    if (request.templateFile is not null)
                    {
                        await new SMSExtension(_staffDbContext, _dbContext, _smsService, _mailService).HandleCampaignWithTemplateFile(filtemplateDto);
                        return Shared.Result.Success<string>("Batch SMS Dispatched");
                    }

                    #region
                    // Handle Staff SMS Request
                    if (request.staffIds is not null)
                    {
                        using (var transaction = await _dbContext.Database.BeginTransactionAsync())
                        {
                            try
                            {
                                await new SMSRequestHandlers(_dbContext, _staffDbContext, _smsService, _mailService).handleOnStaffSMSRequest(request.staffIds, NoFileRequest);
                                await transaction.CommitAsync();
                                return Shared.Result.Success<string>("Batch SMS Dispatched");
                            }
                            catch (Exception ex)
                            {
                                await transaction.RollbackAsync();
                                throw ex;
                            }
                        }
                    }
                    #endregion


                    #region
                    // Handle Unit SMS Request
                    if (request.unitId is not null)
                    {
                        using (var transaction = await _dbContext.Database.BeginTransactionAsync())
                        {
                            try
                            {
                                await new SMSRequestHandlers(_dbContext, _staffDbContext, _smsService, _mailService).handleOnUnitSMSRequest(request.unitId, NoFileRequest);
                                await transaction.CommitAsync();
                                return Shared.Result.Success<string>("Batch SMS Dispatched");
                            }
                            catch (Exception ex)
                            {
                                await transaction.RollbackAsync();
                                throw ex;
                            }
                        }
                    }
                    #endregion

                    #region
                    // Handle Unit SMS Request
                    if (request.departmentId is not null)
                    {
                        using (var transaction = await _dbContext.Database.BeginTransactionAsync())
                        {
                            try
                            {
                                await new SMSRequestHandlers(_dbContext, _staffDbContext, _smsService, _mailService).handleOnDepartmentSMSRequest(request.departmentId, NoFileRequest);
                                await transaction.CommitAsync();
                                return Shared.Result.Success<string>("Batch SMS Dispatched");
                            }
                            catch (Exception ex)
                            {
                                await transaction.RollbackAsync();
                                throw ex;
                            }
                        }
                    }
                    #endregion 

                }
                catch (Exception ex)
                {
                    return Shared.Result.Failure<string>(Error.BadRequest(ex.Message));
                }
                return Shared.Result.Success<string>("Batch SMS Dispatched");

            }
        }
    }
}

public class MapCreateSMSEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/sms-campaign/create", async (ISender sender,
            [FromForm] string campaingName,
            [FromForm] string? smsTemplateId,
            [FromForm] IFormFile? templateFile,
            [FromForm] string? message,
            [FromForm] string? frequency,
            [FromForm] Guid[]? staffIds,
            [FromForm] string? directorateId,
            [FromForm] string? departmentId,
            [FromForm] string? unitId
            ) =>
        {
            var response = await sender.Send(
                new CreateSMSCampaignRequest
                {
                    campaingName = campaingName,
                    smsTemplateId = String.IsNullOrWhiteSpace(smsTemplateId) ? null : Guid.Parse(smsTemplateId),
                    templateFile = templateFile,
                    message = message,
                    frequency = frequency,
                    staffIds = staffIds,
                    directorateId = null,
                    unitId = null
                }
                );
            if (response.IsFailure)
            {
                return Results.UnprocessableEntity(response.Error);
            }

            return Results.Ok(response.Value);

        }).WithTags("SMS-Campaign")
              .WithMetadata(new ProducesResponseTypeAttribute(StatusCodes.Status200OK))
              .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest))
              .DisableAntiforgery();
        ;
    }
}
