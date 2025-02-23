using Carter;
using FluentValidation;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Extensions;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Category.CreateCategory;
using static HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Category.EditCategory;

namespace HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Category
{
    public static class EditCategory
    {
        public record UpdateCategoryRequest : IRequest<Shared.Result>
        {
            public Guid Id { get; set; }
            public string categoryName { get; set; } = String.Empty;

        }


        public class Validator : AbstractValidator<UpdateCategoryRequest>
        {
            private readonly IServiceScopeFactory _scopeFactory;
            public Validator(IServiceScopeFactory scopefactory)
            {
                _scopeFactory = scopefactory;
                RuleFor(c => c.categoryName)
                    .NotEmpty()
                    .MustAsync(async (model, name, cancellationToken) =>
                    {
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var dbContext = scope.ServiceProvider.GetRequiredService<HRMDBContext>();
                            var exist = await dbContext.Category.AnyAsync(c => c.categoryName.ToLower() == name.ToLower() && c.Id != model.Id);
                            return !exist;
                        }
                    })
                    .WithMessage("Category Name Is Already Taken")
                    ;
            }

        }


        internal sealed class Handler : IRequestHandler<UpdateCategoryRequest, Shared.Result>
        {
            private readonly HRMDBContext _dbContext;
            private readonly IValidator<UpdateCategoryRequest> _validator;
            public Handler(HRMDBContext dbContext, IValidator<UpdateCategoryRequest> validator)
            {

                _dbContext = dbContext;
                _validator = validator;

            }
            public async Task<Shared.Result> Handle(UpdateCategoryRequest request, CancellationToken cancellationToken)
            {
                var validationResponse = await _validator.ValidateAsync(request);

                if (validationResponse.IsValid is false)
                {
                    return Shared.Result.Failure(Error.ValidationError(validationResponse));
                }

                var affectedRows = await _dbContext.Category.Where(x => x.Id == request.Id).ExecuteUpdateAsync(setters =>
                    setters.SetProperty(c => c.categoryName, request.categoryName)
                    .SetProperty(c => c.updatedAt, DateTime.UtcNow)
                );

                if (affectedRows >= 1) return Shared.Result.Success();

                return Shared.Result.Failure(Error.CreateNotFoundError("Category To Update Not Found"));
            }
        }

    }



}


public class MapUpdateCategoryEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("api/category/{Id}", async (ISender sender, Guid Id, CreateCategoryRequest request) =>
        {
            var response = await sender.Send(
                new UpdateCategoryRequest
                {
                    Id = Id,
                    categoryName = request.categoryName
                }
                );

            if (response.IsFailure)
            {
                return Results.UnprocessableEntity(response.Error);
            }
            if (response.IsSuccess)
            {
                return Results.NoContent();
            }

            return Results.BadRequest();

        }).WithTags("Setup-Category")
              .WithMetadata(new ProducesResponseTypeAttribute(StatusCodes.Status204NoContent))
              .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest))
              .WithGroupName(SwaggerDoc.SwaggerEndpointDefintions.Setup)
              ;
    }
}