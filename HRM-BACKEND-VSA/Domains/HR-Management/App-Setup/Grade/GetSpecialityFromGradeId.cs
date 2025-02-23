using Carter;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Entities;
using HRM_BACKEND_VSA.Extensions;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Grade.GetSpecialityFromGradeId;

namespace HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Grade
{
    public static class GetSpecialityFromGradeId
    {
        public class GetSpecialityFromGradeIdRequest : IRequest<Shared.Result<ICollection<Entities.Speciality>>>
        {
            public Guid gradeId { get; set; }
        }

        internal sealed class Handler(HRMDBContext dbContext) : IRequestHandler<GetSpecialityFromGradeIdRequest, Shared.Result<ICollection<Entities.Speciality>>>
        {
            async Task<Result<ICollection<Speciality>>> IRequestHandler<GetSpecialityFromGradeIdRequest, Result<ICollection<Speciality>>>.Handle(GetSpecialityFromGradeIdRequest request, CancellationToken cancellationToken)
            {
                Console.WriteLine(request.gradeId);

                var grade = await dbContext.Grade
                    .FirstOrDefaultAsync(g => g.Id == request.gradeId);


                if (grade == null)
                {
                    return Shared.Result.Failure<ICollection<Entities.Speciality>>(Error.CreateNotFoundError("Grade Not Found"));
                }

                var response = await dbContext.Speciality.Where(s => s.categoryId == grade.categoryId).ToListAsync(cancellationToken);

                return Shared.Result.Success<ICollection<Entities.Speciality>>(response);

            }
        }
    }
}

public class MapGetSpecialityFromGradeId : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/grade/speciality-from/{gradeId}/all", async (ISender sender, Guid gradeId) =>
        {
            var response = await sender.Send(
                    new GetSpecialityFromGradeIdRequest
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

        }).WithTags("Setup-Staff-Speciality")
               .WithDescription("Get Specialities list from grade id")
              .WithMetadata(new ProducesResponseTypeAttribute(typeof(Guid), StatusCodes.Status200OK))
              .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest))
              .WithGroupName(SwaggerDoc.SwaggerEndpointDefintions.Setup)
               ;
    }
}