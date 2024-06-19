using AutoMapper;
using Carter;
using FluentValidation;
using HRM_BACKEND_VSA.Contracts;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Domains.Staffs.Services;
using HRM_BACKEND_VSA.Providers;
using HRM_BACKEND_VSA.Serivices.ImageKit;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Domains.Applicant.Applicant_BioData.AddAttachments;

namespace HRM_BACKEND_VSA.Domains.Applicant.Applicant_BioData
{
    public static class AddAttachments
    {
        public class ApplicantAttachmentRequest : IRequest<Shared.Result>
        {
            public IFormFile passportPicture { get; set; }
            public IFormFile birthCertificate { get; set; }
            public IFormFile highestQualificationCertificate { get; set; }
            public IFormFile nssCertificate { get; set; }
        }

        public class Validator : AbstractValidator<ApplicantAttachmentRequest>
        {

            public Validator()
            {

                RuleFor(c => c.passportPicture).NotEmpty();
                RuleFor(c => c.birthCertificate).NotEmpty();
                RuleFor(c => c.highestQualificationCertificate).NotEmpty();
                RuleFor(c => c.nssCertificate).NotEmpty();
            }
        }

        public class Handler : IRequestHandler<ApplicantAttachmentRequest, Shared.Result>
        {
            private readonly HRMStaffDBContext _dbContext;
            private readonly HRMDBContext _hrmManagementdBContext;
            private readonly IMapper _mapper;
            private readonly IValidator<ApplicantAttachmentRequest> _validator;
            private readonly Authprovider _authProvider;
            private readonly ImageKit _imageKit;
            RequestService _requestService;

            public Handler(
                HRMStaffDBContext dBContext,
                IMapper mapper, IValidator<ApplicantAttachmentRequest> validator,
                Authprovider authProvider, ImageKit imageKit, HRMDBContext hrmManagementdBContext, RequestService requestService)
            {
                _dbContext = dBContext;
                _mapper = mapper;
                _validator = validator;
                _authProvider = authProvider;
                _imageKit = imageKit;
                _hrmManagementdBContext = hrmManagementdBContext;
                _requestService = requestService;
            }
            public async Task<Shared.Result> Handle(ApplicantAttachmentRequest request, CancellationToken cancellationToken)
            {
                List<string> imagesUrlList = new() { };

                var validationResult = _validator.Validate(request);

                if (validationResult.IsValid is false)
                {
                    return Shared.Result.Failure<Guid>(Error.ValidationError(validationResult));
                }

                try
                {

                    var authApplicant = await _authProvider.GetAuthApplicant();
                    if (authApplicant is null) return Shared.Result.Failure<Guid>(Error.CreateNotFoundError("Applicant Not Found"));

                    var applicantBioData = await _dbContext.ApplicantBioData.FirstOrDefaultAsync(x => x.applicantId == authApplicant.Id);

                    if (applicantBioData is null) return Shared.Result.Failure<Guid>(Error.CreateNotFoundError("Applicant Not Found"));


                    var passportUploadResponse = await _imageKit.HandleNewFormFileUploadAsync(request.passportPicture);
                    applicantBioData.passportPicture = passportUploadResponse.thumbnailUrl;
                    imagesUrlList.Add(passportUploadResponse.thumbnailUrl);
                    Console.WriteLine($"passsport Pic Url generated as {passportUploadResponse.thumbnailUrl}");

                    var birthCertificateUploadResponse = await _imageKit.HandleNewFormFileUploadAsync(request.birthCertificate);
                    applicantBioData.birthCertificate = birthCertificateUploadResponse.thumbnailUrl;
                    imagesUrlList.Add(birthCertificateUploadResponse.thumbnailUrl);
                    Console.WriteLine($"Birth Cert Pic Url generated as {birthCertificateUploadResponse.thumbnailUrl}");

                    var educationCertificateUploadResponse = await _imageKit.HandleNewFormFileUploadAsync(request.highestQualificationCertificate);
                    applicantBioData.highestQualificationCertificate = educationCertificateUploadResponse.thumbnailUrl;
                    imagesUrlList.Add(educationCertificateUploadResponse.thumbnailUrl);
                    Console.WriteLine($"Educational Cert Pic Url generated as {educationCertificateUploadResponse.thumbnailUrl}");

                    var nssCertificate = await _imageKit.HandleNewFormFileUploadAsync(request.nssCertificate);
                    applicantBioData.nssCertificate = nssCertificate.thumbnailUrl;
                    imagesUrlList.Add(nssCertificate.thumbnailUrl);
                    Console.WriteLine($"nss Cert Pic Url generated as {nssCertificate.thumbnailUrl}");

                    var newApplicationService = _requestService
                        .getRequestService(RegisterationRequestTypes.newRegisteration);

                    if (newApplicationService is not null)
                    {
                        var newApplication = await newApplicationService.NewStaffRequest(authApplicant.Id);
                    }

                    //Updating Application Data Submission status
                    var ApplicantCredentials = await _dbContext.Applicant.FirstOrDefaultAsync(x => x.Id == authApplicant.Id);
                    if (ApplicantCredentials is not null)
                    {
                        ApplicantCredentials.hasSubmittedApplication = true;
                    }

                    await _dbContext.SaveChangesAsync();
                    return Shared.Result.Success();
                }
                catch (Exception ex)
                {
                    return Shared.Result.Failure(Error.BadRequest(ex.Message));
                }

            }
        }
    }
}


public class MapAddApplicantAttachmentEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/applicant/attachments",
         [Authorize(Policy = AuthorizationDecisionType.Applicant)]
        async (ISender sender,
            [FromForm] IFormFile passportPicture,
            [FromForm] IFormFile birthCertificate,
            [FromForm] IFormFile highestQualificationCertificate,
            [FromForm] IFormFile nssCertificate
            ) =>
        {

            var response = await sender.Send(new ApplicantAttachmentRequest
            {
                passportPicture = passportPicture,
                birthCertificate = birthCertificate,
                highestQualificationCertificate = highestQualificationCertificate,
                nssCertificate = nssCertificate
            });

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
        .DisableAntiforgery();
        ;
    }
}

