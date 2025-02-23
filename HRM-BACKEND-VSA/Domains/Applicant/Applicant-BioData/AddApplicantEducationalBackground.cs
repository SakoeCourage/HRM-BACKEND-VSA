using AutoMapper;
using Carter;
using FluentValidation;
using HRM_BACKEND_VSA.Contracts;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Entities.Applicant;
using HRM_BACKEND_VSA.Extensions;
using HRM_BACKEND_VSA.Providers;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Domains.Applicant.Applicant_BioData.AddApplicantEducationalBackground;

namespace HRM_BACKEND_VSA.Domains.Applicant.Applicant_BioData
{
    public static class AddApplicantEducationalBackground
    {

        public class AddEducationalBackgrounRequest : IRequest<Shared.Result>
        {
            public IEnumerable<EducationalBackgroudRequestData> educationalBackground { get; set; }
            public DateOnly yearOfService { get; set; }
            public string? nssNumber { get; set; }
            public string? placeOfService { get; set; }
            public string? highestQualification { get; set; }


        }

        public class EducationalBackgroudRequestDataValidator : AbstractValidator<EducationalBackgroudRequestData>
        {
            public EducationalBackgroudRequestDataValidator()
            {
                RuleFor(x => x.institutionName).NotEmpty().WithMessage("Institution name cannot be empty.");
                RuleFor(x => x.certificate).NotEmpty().WithMessage("Certificate cannot be empty.");
            }
        }

        public class Validator : AbstractValidator<AddEducationalBackgrounRequest>
        {
            public Validator()
            {
                RuleFor(c => c.nssNumber).NotEmpty();
                RuleFor(c => c.placeOfService).NotEmpty();
                RuleFor(c => c.yearOfService).NotEmpty();
                RuleFor(c => c.highestQualification).NotEmpty();
                RuleForEach(x => x.educationalBackground)
                .NotEmpty()
                .WithMessage("Educational Background entry must not be empty.")
                .SetValidator(new EducationalBackgroudRequestDataValidator())
                .Must((request, educationalBackground, context) =>
                 {
                     var distinctEntries = new HashSet<string>();
                     foreach (var entry in request.educationalBackground)
                     {
                         var key = $"{entry.institutionName}-{entry.certificate}";
                         if (!distinctEntries.Add(key))
                         {
                             context.AddFailure($"Duplicate entry: {entry.institutionName} - {entry.certificate}");
                             return false;
                         }
                     }
                     return true;
                 }).WithMessage("Educational Background entry must be unique");
            }
        }
        internal sealed class Handler : IRequestHandler<AddEducationalBackgrounRequest, Shared.Result>
        {

            private readonly HRMStaffDBContext _dbContext;
            private readonly IMapper _mapper;
            private readonly IValidator<AddEducationalBackgrounRequest> _validator;
            private readonly Authprovider _authProvider;

            public Handler(HRMStaffDBContext dBContext, IMapper mapper, IValidator<AddEducationalBackgrounRequest> validator, Authprovider authProvider)
            {
                _dbContext = dBContext;
                _mapper = mapper;
                _validator = validator;
                _authProvider = authProvider;
            }
            public async Task<Shared.Result> Handle(AddEducationalBackgrounRequest request, CancellationToken cancellationToken)
            {

                var validationResult = await _validator.ValidateAsync(request);

                if (validationResult.IsValid is false)
                {
                    return Shared.Result.Failure<Guid>(Error.ValidationError(validationResult));
                }

                var authApplicant = await _authProvider.GetAuthApplicant();
                if (authApplicant is null) return Shared.Result.Failure<Guid>(Error.CreateNotFoundError("Applicant Not Found"));

                var applicantBioData = await _dbContext.ApplicantBioData.FirstOrDefaultAsync(x => x.applicantId == authApplicant.Id);

                if (applicantBioData is null) return Shared.Result.Failure<Guid>(Error.CreateNotFoundError("Applicant Not Found"));

                applicantBioData.yearOfService = request.yearOfService;
                applicantBioData.nssNumber = request.nssNumber;
                applicantBioData.placeOfService = request.placeOfService;
                applicantBioData.highestQualification = request.highestQualification;

                var EducationalBackagroundList = request.educationalBackground.Select(entry => new ApplicantEducationalBackground
                {
                    createdAt = DateTime.UtcNow,
                    updatedAt = DateTime.UtcNow,
                    applicantBioDataId = applicantBioData.Id,
                    yearCompleted = entry.yearCompleted,
                    institutionName = entry.institutionName,
                    certificate = entry.certificate
                });

                _dbContext.ApplicantEducationalBackground.AddRange(EducationalBackagroundList);
                await _dbContext.SaveChangesAsync();
                return Shared.Result.Success();
            }
        }
    }
}


public class MapAddApplicantBioDataEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/applicant/educational-background",
         [Authorize(Policy = AuthorizationDecisionType.Applicant)]
        async (ISender sender, AddEducationalBackgrounRequest request) =>
        {

            var response = await sender.Send(request);

            if (response.IsFailure)
            {
                return Results.UnprocessableEntity(response.Error);
            }
            if (response.IsSuccess)
            {
                return Results.NoContent();
            }

            return Results.BadRequest();
        }).WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status422UnprocessableEntity))
        .WithMetadata(new ProducesResponseTypeAttribute(typeof(Guid), StatusCodes.Status204NoContent))
        .WithTags("Applicant Bio Data")
        .WithGroupName(SwaggerDoc.SwaggerEndpointDefintions.ApplicantService)
            ;
    }
}
