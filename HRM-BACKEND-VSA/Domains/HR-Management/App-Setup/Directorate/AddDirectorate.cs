using Carter;
using FluentValidation;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Directorate.AddDirectorate;

namespace HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Directorate
{
    public static class AddDirectorate
    {

        public class AddDirectorateRequest : IRequest<Shared.Result<Guid>>
        {
            public string directorateName { get; set; }
            public Guid? directorId { get; set; }
            public Guid? depDirectoryId { get; set; }
        }

        public class Validator : AbstractValidator<AddDirectorateRequest>
        {
            private readonly IServiceScopeFactory _scopeFactory;
            public Validator(IServiceScopeFactory scopefactory)
            {
                _scopeFactory = scopefactory;
                RuleFor(c => c.directorateName)
                    .NotEmpty()
                    .MustAsync(async (name, cancellationToken) =>
                    {
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var dbContext = scope.ServiceProvider.GetRequiredService<HRMDBContext>();
                            var exist = await dbContext.Directorate.AnyAsync(c => c.directorateName.ToLower() == name.ToLower());
                            return !exist;
                        }
                    })
                    .WithMessage("Directorate Name Is Already Taken");

            }
        }

        internal sealed class Handler : IRequestHandler<AddDirectorateRequest, Shared.Result<Guid>>
        {
            private readonly HRMDBContext _dbContext;
            private readonly IValidator<AddDirectorateRequest> _validator;

            public Handler(HRMDBContext dbContext, IValidator<AddDirectorateRequest> validator)
            {
                _dbContext = dbContext;
                _validator = validator;
            }
            public async Task<Result<Guid>> Handle(AddDirectorateRequest request, CancellationToken cancellationToken)
            {

                var validationResult = await _validator.ValidateAsync(request, cancellationToken);

                if (validationResult.IsValid is false)
                {
                    return Shared.Result.Failure<Guid>(Error.ValidationError(validationResult));
                }

                var newEntry = new Entities.Directorate()
                {
                    createdAt = DateTime.UtcNow,
                    updatedAt = DateTime.UtcNow,
                    directorateName = request.directorateName,
                    depDirectoryId = request.directorId,
                    directorId = request.directorId
                };

                _dbContext.Add(newEntry);

                await _dbContext.SaveChangesAsync();
                return Shared.Result.Success(newEntry.Id);
            }
        }
    }
}

public class MapAddDirectorateEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/directorate", async (ISender sender, AddDirectorateRequest request) =>
        {
            var response = await sender.Send(request);
            if (response.IsFailure)
            {
                return Results.UnprocessableEntity(response.Error);
            }
            if (response.IsSuccess)
            {
                return Results.Ok(response.Value);
            }
            return Results.BadRequest();

        }).WithTags("Setup-Directorate")
              .WithMetadata(new ProducesResponseTypeAttribute(StatusCodes.Status200OK))
              .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest));
    }
}
