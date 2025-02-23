using Carter;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Entities;
using HRM_BACKEND_VSA.Extensions;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Grade.GetCategoryFromGradeId;

namespace HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Grade
{
    public static class GetCategoryFromGradeId
    {
        public class GetCateogoryFromGradeIdRequest : IRequest<Shared.Result<object>>
        {
            public Guid gradeId { get; set; }
        }

        internal sealed class Handler(HRMDBContext dbContext) : IRequestHandler<GetCateogoryFromGradeIdRequest, Shared.Result<object>>
        {
            public async Task<Result<object>> Handle(GetCateogoryFromGradeIdRequest request, CancellationToken cancellationToken)
            {
                var response = await dbContext
                    .Grade
                    .Where(x => x.Id == request.gradeId)
                    .Include(gr => gr.steps)
                    .Join(dbContext.Category.Include(x => x.specialities),
                      gr => gr.categoryId,
                      ct => ct.Id,
                      (gr, ct) => new
                      {
                          Grade = gr,
                          Category = ct
                      }
                    )
                    .Select(entry =>
                    new
                    {
                        id = entry.Category.Id,
                        categoryName = entry.Category.categoryName,
                        gradeName = entry.Grade.gradeName,
                        level = entry.Grade.level,
                        scale = entry.Grade.scale,
                        steps = entry.Grade.steps,
                        specialities = entry.Category.specialities
                    }
                    )
                    .ToListAsync();

                return Shared.Result.Success<object>(response.Count > 0 ? response.First() : response);

            }
        }
    }
}

public class MapGetCategoryFromGradeId : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/grade/category-from/{gradeId}/all", async (ISender sender, Guid gradeId) =>
        {
            var response = await sender.Send(
                    new GetCateogoryFromGradeIdRequest
                    {
                        gradeId = gradeId
                    }
                );

            if (response.IsFailure)
            {
                return Results.UnprocessableEntity(response.Error);
            }

            if (response.IsSuccess)
            {
                return Results.Ok(response.Value);
            };

            return Results.BadRequest();

        }).WithTags("Setup-Grade")
               .WithDescription("Get category and specialty list from grade id")
              .WithMetadata(new ProducesResponseTypeAttribute(typeof(Category), StatusCodes.Status200OK))
              .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest))
              .WithGroupName(SwaggerDoc.SwaggerEndpointDefintions.Setup)
               ;
    }
}
