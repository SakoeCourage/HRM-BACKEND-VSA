using AutoMapper;
using Carter;
using FluentValidation;
using HRM_BACKEND_VSA.Contracts;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Providers;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using static HRM_BACKEND_VSA.Features.Applicant.ApplicantLogin;



namespace HRM_BACKEND_VSA.Features.Applicant
{
    public static class ApplicantLogin
    {
        public class ApplicantLoginRequest : IRequest<Result<ApplicantLoginResponse>>
        {
            [Required]
            public string contact { get; set; }

            [Required]
            public string otp { get; set; }
        }

        public class Validator : AbstractValidator<ApplicantLoginRequest>
        {
            public Validator()
            {
                RuleFor(c => c.contact)
                    .NotEmpty()
                    .MinimumLength(9);
                RuleFor(c => c.otp)
                    .NotEmpty()
                    .MinimumLength(4);
            }

        }

        public class Handler : IRequestHandler<ApplicantLoginRequest, Result<ApplicantLoginResponse>>
        {

            private readonly HRMStaffDBContext _dbContext;
            private readonly IValidator<ApplicantLoginRequest> _validator;
            private readonly IMapper _mapper;
            private readonly JWTProvider _jwtProvider;
            public Handler(HRMStaffDBContext dbContext, IValidator<ApplicantLoginRequest> validator, IMapper mapper, JWTProvider jwtProvider)
            {
                _dbContext = dbContext;
                _validator = validator;
                _mapper = mapper;
                _jwtProvider = jwtProvider;
            }
            public async Task<Result<ApplicantLoginResponse>> Handle(ApplicantLoginRequest request, CancellationToken cancellationToken)
            {
                var validationResult = _validator.Validate(request);

                if (validationResult.IsValid is false)
                {
                    return Shared.Result.Failure<ApplicantLoginResponse>(Error.ValidationError(validationResult));
                }
                var hasOTP = await _dbContext
                            .ApplicantHasOTP
                            .Include(a => a.applicant)
                            .FirstOrDefaultAsync(a => a.contact == request.contact && a.otp == request.otp);

                if (hasOTP == null) return Shared.Result.Failure<ApplicantLoginResponse>(Error.CreateNotFoundError("Failed To Confirm OTP"));
                DateTime OTPCreatedDate = hasOTP.updatedAt;
                bool hasExpired = DateTime.UtcNow - OTPCreatedDate > TimeSpan.FromMinutes(10);

                if (hasExpired) return Shared.Result.Failure<ApplicantLoginResponse>(Error.BadRequest("OTP Has Expired"));

                var response = _mapper.Map<ApplicantLoginResponse>(hasOTP.applicant);
                response.accessToken = _jwtProvider.GenerateAccessToken(response.Id, AuthorizationDecisionType.Applicant);
                await _dbContext.ApplicantHasOTP.Where(x => x.contact == hasOTP.contact).ExecuteDeleteAsync();
                return Shared.Result.Success(response);
            }
        }
    }


}

public class MappApplicantLoginEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/applicant/login", async (ISender sender, ApplicantLoginRequest request) =>
        {
            var result = await sender.Send(request);

            if (result.IsFailure)
            {
                return Results.BadRequest(result?.Error);
            }
            if (result.IsSuccess)
            {
                return Results.Ok(result?.Value);
            }

            return Results.BadRequest();
        })
        .WithMetadata(new ProducesResponseTypeAttribute(typeof(ApplicantLoginResponse), StatusCodes.Status200OK))
        .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status422UnprocessableEntity))
        .WithTags("Authentication Applicant");
    }
}
