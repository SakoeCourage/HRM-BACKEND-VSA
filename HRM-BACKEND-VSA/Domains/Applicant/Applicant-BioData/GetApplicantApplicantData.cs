using AutoMapper;
using Carter;
using FluentValidation;
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
using static HRM_BACKEND_VSA.Domains.Applicant.Applicant_BioData.GetApplicantApplicantData;

namespace HRM_BACKEND_VSA.Domains.Applicant.Applicant_BioData
{
    public static class GetApplicantApplicantData
    {
        public class GetApplicationDataRequest : IRequest<Shared.Result<ApplicantBioData>>
        {


        }

        internal sealed class Handler : IRequestHandler<GetApplicationDataRequest, Shared.Result<ApplicantBioData>>
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
            public async Task<Result<ApplicantBioData>> Handle(GetApplicationDataRequest request, CancellationToken cancellationToken)
            {

                var authApplicant = await _authProvider.GetAuthApplicant();
                if (authApplicant is null) return Shared.Result.Failure<ApplicantBioData>(Error.CreateNotFoundError("Applicant Not Found"));

                var applicantBioData = await _dbContext.ApplicantBioData.Include(a => a.educationalBackground).FirstOrDefaultAsync(x => x.applicantId == authApplicant.Id);

                if (applicantBioData is null) return Shared.Result.Failure<ApplicantBioData>(Error.CreateNotFoundError("Applicant Bio Data Found"));

                return Shared.Result.Success(applicantBioData);
            }
        }
    }
}


public class MappGetApplicantApplicantDataEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/applicant/bio-data",
         [Authorize(Policy = AuthorizationDecisionType.Applicant)]
        async (ISender sender) =>
        {
            var response = await sender.Send(new GetApplicationDataRequest { });

            if (response.IsFailure)
            {
                return Results.NotFound(response.Error);
            }

            if (response.IsSuccess)
            {
                return Results.Ok(response?.Value);
            }
            return Results.BadRequest();
        }).WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status422UnprocessableEntity))
        .WithMetadata(new ProducesResponseTypeAttribute(typeof(ApplicantBioData), StatusCodes.Status200OK))
        .WithTags("Applicant Bio Data")
        .DisableAntiforgery()
        .WithGroupName(SwaggerDoc.SwaggerEndpointDefintions.ApplicantService)
        ;
    }
}

