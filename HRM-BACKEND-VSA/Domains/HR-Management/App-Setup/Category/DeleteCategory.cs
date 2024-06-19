using Carter;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Category.DeleteCategory;

namespace HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Category
{
    public static class DeleteCategory
    {

        public class DeleteCategoryRequest : IRequest<Shared.Result>
        {
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<DeleteCategoryRequest, Shared.Result>
        {
            private readonly HRMDBContext _dbContext;
            public Handler(HRMDBContext dbContext)
            {

                _dbContext = dbContext;

            }

            public async Task<Shared.Result> Handle(DeleteCategoryRequest request, CancellationToken cancellationToken)
            {
                var affectedRows = await _dbContext
                    .Category
                    .Where(s => s.Id == request.Id)
                    .ExecuteDeleteAsync();

                if (affectedRows == 0) return Shared.Result.Failure(Error.NotFound);
                return Shared.Result.Success();
            }
        }
    }
}


public class MapDeleteCategoryEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/category/{Id}", async (ISender sender, Guid Id) =>
        {
            var response = await sender.Send(new DeleteCategoryRequest { Id = Id });

            if (response.IsFailure)
            {
                return Results.NotFound(response.Error);
            }

            return Results.NoContent();

        }).WithTags("Setup-Category")
              .WithMetadata(new ProducesResponseTypeAttribute(StatusCodes.Status204NoContent))
              .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest))
          ;
    }
}