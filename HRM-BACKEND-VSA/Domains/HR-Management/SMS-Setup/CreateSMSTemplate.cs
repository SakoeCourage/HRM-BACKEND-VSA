using Carter;
using FluentValidation;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Model.SMS;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using HRM_BACKEND_VSA.Extensions;
using static HRM_BACKEND_VSA.Features.SMS_Setup.CreateSMSTemplate;

namespace HRM_BACKEND_VSA.Features.SMS_Setup
{
    public static class CreateSMSTemplate
    {
        public class NewSMSTemplateRequest : IRequest<Result<Guid>>
        {
            public string name { get; set; }
            [Required]
            public string message { get; set; }

            public string? description { get; set; }
        }

        public class Validator : AbstractValidator<NewSMSTemplateRequest>
        {
            private readonly IServiceScopeFactory _scopeFactory;
            public Validator(IServiceScopeFactory scopeFactory)
            {
                _scopeFactory = scopeFactory;
                RuleFor(c => c.name)
                    .NotEmpty()
                    .MustAsync(async (name, cancellationToken) =>
                    {
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var dbContext = scope.ServiceProvider.GetRequiredService<HRMDBContext>();
                            var exist = await dbContext.SMSTemplate.AnyAsync(c => c.name.ToLower() == name.ToLower());
                            return !exist;
                        }
                    });
                RuleFor(c => c.message)
                    .NotEmpty()
                    .MinimumLength(5);
            }
        }

        internal sealed class Handler : IRequestHandler<NewSMSTemplateRequest, Result<Guid>>
        {
            private readonly IValidator<NewSMSTemplateRequest> _validator;
            private readonly HRMDBContext _dbContext;
            public Handler(IValidator<NewSMSTemplateRequest> validator, HRMDBContext dbContext)
            {
                _validator = validator;
                _dbContext = dbContext;
            }
            public async Task<Result<Guid>> Handle(NewSMSTemplateRequest request, CancellationToken cancellationToken)
            {
                var validationResponse = await _validator.ValidateAsync(request);

                if (validationResponse.IsValid is false)
                {
                    return Shared.Result.Failure<Guid>(Shared.Error.ValidationError(validationResponse));
                }

                var newData = new SMSTemplate
                {
                    name = request.name,
                    description = request.description,
                    message = request.message,
                    createdAt = DateTime.UtcNow,
                    updatedAt = DateTime.UtcNow,
                    readOnly = false
                };

                await _dbContext.SMSTemplate.AddAsync(newData);
                await _dbContext.SaveChangesAsync();

                return Shared.Result.Success<Guid>(newData.Id);
            }
        }
    }
}

public class MapCreateSMSTemplateEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/sms-template", async (NewSMSTemplateRequest request, ISender sender) =>
        {
            var result = await sender.Send(request);
            if (result.IsFailure)
            {
                return Results.UnprocessableEntity(result.Error);
            };

            return Results.Ok(result.Value);
        })
        .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status422UnprocessableEntity))
        .WithMetadata(new ProducesResponseTypeAttribute(typeof(Guid), StatusCodes.Status200OK))
        .WithTags("Setup-SMS Template")
        .WithGroupName(SwaggerDoc.SwaggerEndpointDefintions.SMSService)
        ;
    }
}
