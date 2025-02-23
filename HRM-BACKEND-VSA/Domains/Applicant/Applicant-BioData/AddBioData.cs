using AutoMapper;
using Carter;
using FluentValidation;
using HRM_BACKEND_VSA.Contracts;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Entities.Applicant;
using HRM_BACKEND_VSA.Providers;
using HRM_BACKEND_VSA.Serivices.ImageKit;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using HRM_BACKEND_VSA.Extensions;
using static HRM_BACKEND_VSA.Domains.Applicant.Applicant_BioData.AddBioData;

namespace HRM_BACKEND_VSA.Domains.Applicant.Applicant_BioData
{
    public static class AddBioData
    {
        public class AddBioDataRequest : IRequest<Shared.Result<Guid>>
        {
            public TitleEnum? title { get; set; }
            public string surName { get; set; } = String.Empty;
            public string firstName { get; set; } = String.Empty;
            public string? otherNames { get; set; } = String.Empty;
            public GenderEnum? gender { get; set; }
            public string citizenship { get; set; } = String.Empty;
            public DateOnly dateOfBirth { get; set; }
            public string? SNNITNumber { get; set; } = String.Empty;
            public string phoneOne { get; set; } = String.Empty;
            public string? phoneTwo { get; set; } = String.Empty;
            [EmailAddress]
            public string email { get; set; } = String.Empty;
            public string? GPSAddress { get; set; } = String.Empty;
            public string? disability { get; set; } = String.Empty;
            public string ECOWASCardNumber { get; set; } = String.Empty;
            public string? passportNumber { get; set; } = String.Empty;
            public string? controllerStaffNumber { get; set; } = String.Empty;

        }

        public class Validator : AbstractValidator<AddBioDataRequest>
        {
            private readonly IServiceScopeFactory _serviceScopeFactory;
            public Validator(IServiceScopeFactory serviceScopeFactory)
            {
                _serviceScopeFactory = serviceScopeFactory;
                var scope = _serviceScopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<HRMStaffDBContext>();
                var authProvider = scope.ServiceProvider.GetRequiredService<Authprovider>();


                RuleFor(c => c.title).NotEmpty().Must(t => Enum.IsDefined(typeof(TitleEnum), t)).WithMessage("Invalid Title Selection");
                RuleFor(c => c.firstName).NotEmpty();
                RuleFor(c => c.surName).NotEmpty();

                RuleFor(c => c.gender).NotEmpty().Must(t => Enum.IsDefined(typeof(GenderEnum), t)).WithMessage("Invalid Gender Selection");
                RuleFor(c => c.dateOfBirth).NotEmpty();
                RuleFor(c => c.SNNITNumber).MustAsync(async (no, cancellationToken) =>
                {
                    if (String.IsNullOrWhiteSpace(no))
                    {
                        return true;
                    };
                    var authApplicant = await authProvider.GetAuthApplicant();
                    var exist = await dbContext.ApplicantBioData.AnyAsync(c => c.SNNITNumber.ToLower() == no.ToLower() && c.applicantId != authApplicant.Id);
                    return !exist;
                }).WithMessage("A Registeration With Same SNNIT Number was found");
                RuleFor(c => c.ECOWASCardNumber).MustAsync(async (no, cancellationToken) =>
                {
                    if (String.IsNullOrWhiteSpace(no))
                    {
                        return true;
                    };
                    var authApplicant = await authProvider.GetAuthApplicant();
                    var exist = await dbContext.ApplicantBioData.AnyAsync(c => c.ECOWASCardNumber.ToLower() == no.ToLower() && c.applicantId != authApplicant.Id);
                    return !exist;
                }).WithMessage("A Registeration With Same ECOWAS Card Number was found"); ;
                RuleFor(c => c.passportNumber).MustAsync(async (no, cancellationToken) =>
                {
                    if (String.IsNullOrWhiteSpace(no))
                    {
                        return true;
                    };
                    if (String.IsNullOrWhiteSpace(no)) return true;
                    var authApplicant = await authProvider.GetAuthApplicant();
                    var exist = await dbContext.ApplicantBioData.AnyAsync(c => c.passportNumber.ToLower() == no.ToLower() && c.applicantId != authApplicant.Id);
                    return !exist;
                }).WithMessage("A Registeration With Same Passport Number was found"); ;
                RuleFor(c => c.email).NotEmpty().EmailAddress();

            }

        }

        internal sealed class Handler : IRequestHandler<AddBioDataRequest, Shared.Result<Guid>>
        {

            public class ImageUploadRecord
            {
                public string passportPicUrl { get; set; }
                public string educationalCerticateUrl { get; set; }
                public string birthCertificateUrl { get; set; }
            }

            private readonly ImageKit _imageKit;
            private readonly HRMStaffDBContext _dbContext;
            private readonly IMapper _mapper;
            private readonly IValidator<AddBioDataRequest> _validator;
            private readonly Authprovider _authProvider;
            public Handler(HRMStaffDBContext dBContext, IMapper mapper, IValidator<AddBioDataRequest> validator, ImageKit imageKit, Authprovider authProvider)
            {
                _dbContext = dBContext;
                _mapper = mapper;
                _validator = validator;
                _imageKit = imageKit;
                _authProvider = authProvider;
            }


            public async Task<Result<Guid>> Handle(AddBioDataRequest request, CancellationToken cancellationToken)
            {
                List<string> imagesUrlList = new() { };

                var validationResult = await _validator.ValidateAsync(request);

                if (validationResult.IsValid is false)
                {
                    return Shared.Result.Failure<Guid>(Error.ValidationError(validationResult));
                }

                using (var transaction = await _dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var newBioDataEntry = _mapper.Map<ApplicantBioData>(request);

                        var authApplicant = await _authProvider.GetAuthApplicant();

                        if (authApplicant is null) return Shared.Result.Failure<Guid>(Error.CreateNotFoundError("Applicant Not Found"));

                        var existingApplicantBioData = await _dbContext.ApplicantBioData.FirstOrDefaultAsync(a => a.applicantId == authApplicant.Id);
                        if (existingApplicantBioData is not null)
                        {
                            existingApplicantBioData.title = newBioDataEntry.title;
                            existingApplicantBioData.email = newBioDataEntry.email;
                            existingApplicantBioData.gender = newBioDataEntry.gender;
                            existingApplicantBioData.updatedAt = DateTime.UtcNow;
                            existingApplicantBioData.citizenship = newBioDataEntry.citizenship;
                            existingApplicantBioData.dateOfBirth = newBioDataEntry.dateOfBirth;
                            existingApplicantBioData.SNNITNumber = newBioDataEntry.SNNITNumber;
                            existingApplicantBioData.ECOWASCardNumber = newBioDataEntry.ECOWASCardNumber;
                            existingApplicantBioData.passportNumber = newBioDataEntry.passportNumber;
                            existingApplicantBioData.firstName = newBioDataEntry.firstName;
                            existingApplicantBioData.surName = newBioDataEntry.surName;
                            existingApplicantBioData.otherNames = newBioDataEntry.otherNames;
                            existingApplicantBioData.phoneOne = newBioDataEntry.phoneOne;
                            existingApplicantBioData.phoneTwo = newBioDataEntry.phoneTwo;
                            existingApplicantBioData.disability = newBioDataEntry.disability;
                            existingApplicantBioData.highestQualification = newBioDataEntry.highestQualification;
                            existingApplicantBioData.GPSAddress = newBioDataEntry.GPSAddress;
                            existingApplicantBioData.controllerStaffNumber = newBioDataEntry.controllerStaffNumber;

                            await _dbContext.SaveChangesAsync(cancellationToken);
                            await transaction.CommitAsync(cancellationToken);
                            return Shared.Result.Success(existingApplicantBioData.Id);
                        }
                        else
                        {
                            newBioDataEntry.applicantId = authApplicant.Id;
                            _dbContext.ApplicantBioData.Add(newBioDataEntry);
                            await _dbContext.SaveChangesAsync(cancellationToken);
                            await transaction.CommitAsync(cancellationToken);
                            return Shared.Result.Success(newBioDataEntry.Id);

                        }
                        // TO-DO Handle on any of the files upload failure and on any database transactions failure delete recently uploaded files
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        await transaction.RollbackAsync();
                        return Shared.Result.Failure<Guid>(Error.BadRequest(ex.Message));
                    }

                }

            }
        }
    }
}


public class MappAddBioDataEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/applicant/bio-data",
        [Authorize(Policy = AuthorizationDecisionType.Applicant)]
        async (ISender sender,
            AddBioDataRequest request
            ) =>
        {
            var response = await sender.Send(request);

            if (response.IsFailure)
            {
                return Results.UnprocessableEntity(response.Error);
            }

            if (response.IsSuccess)
            {
                return Results.Ok(response?.Value);
            }
            return Results.BadRequest();
        }).WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status422UnprocessableEntity))
        .WithMetadata(new ProducesResponseTypeAttribute(typeof(Guid), StatusCodes.Status200OK))
        .WithTags("Applicant Bio Data")
        .DisableAntiforgery()
        .WithGroupName(SwaggerDoc.SwaggerEndpointDefintions.ApplicantService)
        ;
    }
}
