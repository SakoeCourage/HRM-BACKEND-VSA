using Carter;
using FluentValidation;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Features.SMS_Setup.CreateSMSTemplate;
using static HRM_BACKEND_VSA.Features.SMS_Setup.EditSMSTemplate;

namespace HRM_BACKEND_VSA.Features.SMS_Setup
{
    public static class EditSMSTemplate
    {
        public class EditSMSTemplateRequest : IRequest<Shared.Result>
        {
            public Guid Id { get; set; }
            public string name { get; set; }
            public string message { get; set; }
            public string? description { get; set; }
        }
    }

    public class Validator : AbstractValidator<EditSMSTemplateRequest>
    {
        private readonly IServiceScopeFactory _scopeFactory;
        public Validator(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            RuleFor(c => c.name).NotEmpty().
                MustAsync(async (model, name, cancellationToken) =>
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<HRMDBContext>();
                        var exist = await dbContext.SMSTemplate.AnyAsync(c => c.name.ToLower() == name.ToLower() && c.Id != model.Id);
                        return !exist;
                    }
                }).WithMessage("SMS Name is already taken");
            RuleFor(c => c.message)
                .NotEmpty()
                .MinimumLength(5);
        }
    }

    internal sealed class Handler : IRequestHandler<EditSMSTemplateRequest, Shared.Result>
    {
        private readonly IValidator<EditSMSTemplateRequest> _validator;
        private readonly HRMDBContext _dbContext;
        public Handler(IValidator<EditSMSTemplateRequest> validator, HRMDBContext dbContext)
        {
            _validator = validator;
            _dbContext = dbContext;
        }
        public async Task<Shared.Result> Handle(EditSMSTemplateRequest request, CancellationToken cancellationToken)
        {
            var validationResponse = await _validator.ValidateAsync(request, cancellationToken);

            if (validationResponse.IsValid is false)
            {
                return Shared.Result.Failure(Error.ValidationError(validationResponse));
            }

            var affectedRows = await _dbContext.SMSTemplate.Where(x => x.Id == request.Id)
                .ExecuteUpdateAsync(setters =>
                 setters.SetProperty(p => p.updatedAt, DateTime.UtcNow)
                .SetProperty(p => p.description, request.description)
                .SetProperty(p => p.message, request.message)
                .SetProperty(p => p.name, request.name)
                );

            if (affectedRows == 0) return Shared.Result.Failure(Error.NotFound);

            return Shared.Result.Success();
        }
    }
}

public class MapEditSMSTemplateEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("api/sms-template/{Id}", async (ISender sender, Guid Id, NewSMSTemplateRequest request) =>
        {
            var newRequest = new EditSMSTemplateRequest
            {
                Id = Id,
                name = request.name,
                message = request.message,
                description = request.description
            };

            var response = await sender.Send(newRequest);

            if (response.IsFailure)
            {
                return Results.UnprocessableEntity(response.Error);
            };

            if (response.IsSuccess)
            {
                return Results.NoContent();
            }
            return Results.BadRequest();
        })
        .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status422UnprocessableEntity))
        .WithMetadata(new ProducesResponseTypeAttribute(typeof(Guid), StatusCodes.Status200OK))
        .WithTags("Setup-SMS Template");
    }
}
