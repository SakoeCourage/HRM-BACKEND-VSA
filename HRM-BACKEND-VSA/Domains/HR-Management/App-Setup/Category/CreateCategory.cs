using Carter;
using FluentValidation;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Extensions;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Category.CreateCategory;

namespace HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Category
{
    public static class CreateCategory
    {
        public record CreateCategoryRequest : IRequest<Shared.Result<Guid>>
        {
            public string categoryName { get; set; } = String.Empty;
        }

        public class Validator : AbstractValidator<CreateCategoryRequest>
        {
            private readonly IServiceScopeFactory _scopeFactory;
            public Validator(IServiceScopeFactory scopefactory)
            {
                _scopeFactory = scopefactory;
                RuleFor(c => c.categoryName)
                    .NotEmpty()
                    .MustAsync(async (name, cancellationToken) =>
                    {
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var dbContext = scope.ServiceProvider.GetRequiredService<HRMDBContext>();
                            var exist = await dbContext.Category.AnyAsync(c => c.categoryName.ToLower() == name.ToLower());
                            return !exist;
                        }
                    })
                    .WithMessage("Category Name Is Already Taken")
                    ;
            }
        }

        internal sealed class Handler : IRequestHandler<CreateCategoryRequest, Shared.Result<Guid>>
        {
            private readonly HRMDBContext _dbContext;
            private readonly IValidator<CreateCategoryRequest> _validator;
            public Handler(HRMDBContext dbContext, IValidator<CreateCategoryRequest> validator)
            {

                _dbContext = dbContext;
                _validator = validator;

            }
            public async Task<Result<Guid>> Handle(CreateCategoryRequest request, CancellationToken cancellationToken)
            {
                var validationResponse = await _validator.ValidateAsync(request);

                if (validationResponse.IsValid is false)
                {
                    return Shared.Result.Failure<Guid>(Error.ValidationError(validationResponse));
                }

                var newEntry = new Entities.Category
                {
                    createdAt = DateTime.UtcNow,
                    updatedAt = DateTime.UtcNow,
                    categoryName = request.categoryName
                };

                _dbContext.Category.Add(newEntry);
                await _dbContext.SaveChangesAsync();
                return Shared.Result.Success(newEntry.Id);
            }
        }
    }
}

public class MapCreateCategoryEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/category", async (ISender sender, CreateCategoryRequest request) =>
        {
            var response = await sender.Send(request);
            if (response.IsSuccess)
            {
                return Results.Ok(response.Value);
            }
            if (response.IsFailure)
            {
                return Results.UnprocessableEntity(response.Error);
            }

            return Results.BadRequest();
        }).WithTags("Setup-Category").
            WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status422UnprocessableEntity))
          .WithMetadata(new ProducesResponseTypeAttribute(typeof(Guid), StatusCodes.Status200OK))
          .WithGroupName(SwaggerDoc.SwaggerEndpointDefintions.Setup)
            ;
    }
}
