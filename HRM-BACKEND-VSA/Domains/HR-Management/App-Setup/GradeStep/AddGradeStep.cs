using Carter;
using FluentValidation;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Extensions;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.GradeStep.AddGradeStep;

namespace HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.GradeStep
{
    public static class AddGradeStep
    {
        public class StepRequest
        {
            public int stepIndex { get; set; }
            public Double salary { get; set; }
            public Double marketPreBaseSalary { get; set; }
        }

        public class StepsValidator : AbstractValidator<StepRequest>
        {
            public StepsValidator()
            {
                RuleFor(x => x.stepIndex).NotEmpty().WithMessage("Step Index name cannot be empty.");
                RuleFor(x => x.salary).NotEmpty().WithMessage("Salary cannot be empty.");
                RuleFor(x => x.marketPreBaseSalary).NotEmpty().WithMessage("Market Pre Base Salary cannot be empty.");
            }
        }
        public class AddGradeStepRquest : IRequest<Shared.Result>
        {
            public Guid gradeId { get; set; }
            public List<StepRequest> steps { get; set; }
        }



        public class Validator : AbstractValidator<AddGradeStepRquest>
        {
            public Validator()
            {
                RuleForEach(x => x.steps)
                .NotEmpty()
                .WithMessage("Step entry must not be empty.")
                .SetValidator(new StepsValidator())
                .Must((request, stpes, context) =>
                 {
                     var distinctEntries = new HashSet<string>();
                     foreach (var entry in request.steps)
                     {
                         var key = $"{entry.stepIndex}";
                         if (!distinctEntries.Add(key))
                         {
                             context.AddFailure($"Duplicate entry: step at {entry.stepIndex}");
                             return false;
                         }
                     }
                     return true;
                 }).WithMessage("Each Step entry must be unique.");
            }
        }

        public class Hanlder : IRequestHandler<AddGradeStepRquest, Shared.Result>
        {
            private readonly HRMDBContext _dbContext;
            private readonly IValidator<AddGradeStepRquest> _validator;
            public Hanlder(HRMDBContext dbContext, IValidator<AddGradeStepRquest> validator)
            {

                _dbContext = dbContext;
                _validator = validator;

            }
            public async Task<Shared.Result> Handle(AddGradeStepRquest request, CancellationToken cancellationToken)
            {
                var validationResponse = await _validator.ValidateAsync(request);

                if (validationResponse.IsValid is false)
                {
                    return Shared.Result.Failure<Guid>(Shared.Error.ValidationError(validationResponse));
                }

                using (var transaction = await _dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var existingGrade = await _dbContext.Grade.FirstOrDefaultAsync(x => x.Id == request.gradeId);

                        #region

                        if (existingGrade is null) return Shared.Result.Failure(Error.CreateNotFoundError("Request Grade Id Not Found"));
                        if (request.steps.Count() > existingGrade.maximumStep) return Shared.Result.Failure(Error.BadRequest("Grade Index Out Of Range"));

                        #endregion

                        var newEntry = request.steps
                            .Select(entry => new Entities.GradeStep
                            {
                                gradeId = existingGrade.Id,
                                createdAt = DateTime.UtcNow,
                                updatedAt = DateTime.UtcNow,
                                stepIndex = entry.stepIndex,
                                marketPreBaseSalary = entry.marketPreBaseSalary,
                                salary = entry.salary
                            }); ;
                        _dbContext.GradeStep.AddRange(newEntry);
                        await _dbContext.SaveChangesAsync(cancellationToken);
                        await transaction.CommitAsync(cancellationToken);
                        return Shared.Result.Success();

                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        return Shared.Result.Failure<Guid>(Error.BadRequest(ex.Message));

                    }
                }
            }
        }


    }
}


public class MapAddGradeStepsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/grade/{gradeId}/steps", async (ISender sender, Guid gradeId, List<StepRequest> request) =>
        {
            var response = await sender.Send(
            new AddGradeStepRquest
            {
                gradeId = gradeId,
                steps = request
            }
                ); ;
            if (response.IsFailure)
            {
                return Results.UnprocessableEntity(response.Error);
            }

            if (response.IsSuccess)
            {
                return Results.NoContent();
            }
            return Results.BadRequest();

        }).WithTags("Setup-Grade-Step")
              .WithMetadata(new ProducesResponseTypeAttribute(StatusCodes.Status204NoContent))
              .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest))
              .WithGroupName(SwaggerDoc.SwaggerEndpointDefintions.Setup)

          ;
    }
}
