using AutoMapper;
using Carter;
using FluentValidation;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Extensions;
using HRM_BACKEND_VSA.Providers;
using HRM_BACKEND_VSA.Serivices.Mail_Service;
using HRM_BACKEND_VSA.Services.SMS_Service;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Contracts.UserContracts;
using static HRM_BACKEND_VSA.Domains.HR_Management.User.User_Authentication.ConfirmTwoFactorOtp;

namespace HRM_BACKEND_VSA.Domains.HR_Management.User.User_Authentication
{
    public class ConfirmTwoFactorOtp
    {
        public class ConfirmTwoFactorOptRequest : IRequest<Shared.Result<UserLoginResponse>>
        {
            public string email { get; set; }
            public string otp { get; set; }
        }
        public class Validator : AbstractValidator<ConfirmTwoFactorOptRequest>
        {
            public Validator()
            {
                RuleFor(c => c.email)
                    .NotEmpty()
                    .MinimumLength(5);
                RuleFor(c => c.otp)
                    .NotEmpty()
                    .MinimumLength(4);
            }

        }
        internal sealed class Handler : IRequestHandler<ConfirmTwoFactorOptRequest, Shared.Result<UserLoginResponse>>
        {
            private readonly HRMDBContext _dbContext;
            private readonly IMapper _mapper;
            private readonly SMSService _smsService;
            private readonly MailService _mailService;
            private readonly IValidator<ConfirmTwoFactorOptRequest> _validator;
            private readonly JWTProvider _jwtProvider;
            public Handler(JWTProvider jwtProvider, HRMDBContext dbContext, IMapper mapper, SMSService smsService, MailService mailService, IValidator<ConfirmTwoFactorOptRequest> validator)
            {
                _jwtProvider = jwtProvider;
                _dbContext = dbContext;
                _mapper = mapper;
                _smsService = smsService;
                _mailService = mailService;
                _validator = validator;
            }

            public async Task<Result<UserLoginResponse>> Handle(ConfirmTwoFactorOptRequest request, CancellationToken cancellationToken)
            {
                var validationResult = _validator.Validate(request);

                if (validationResult.IsValid is false)
                {
                    return Shared.Result.Failure<UserLoginResponse>(Error.ValidationError(validationResult));
                }
                var hasOTP = await _dbContext
                       .UserHasOTP
                       .FirstOrDefaultAsync(a => a.email == request.email && a.otp == request.otp);

                if (hasOTP == null) return Shared.Result.Failure<UserLoginResponse>(Error.CreateNotFoundError("Failed To Confirm OTP"));
                DateTime OTPCreatedDate = hasOTP.updatedAt;
                bool hasExpired = DateTime.UtcNow - OTPCreatedDate > TimeSpan.FromMinutes(10);

                if (hasExpired) return Shared.Result.Failure<UserLoginResponse>(Error.BadRequest("OTP Has Expired"));

                var user = await _dbContext
                    .User
                    .Include(u => u.staff)
                    .Include(u=>u.role)
                    .IgnoreAutoIncludes()
                    .FirstOrDefaultAsync(u => u.email == hasOTP.email);

                if (user is null) return Shared.Result.Failure<UserLoginResponse>(Error.CreateNotFoundError("User Not Found"));

                user.lastSeen = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();

                var staffLoginResponse = new StaffLoginResponsePartial
                {
                    Id = user.staff.Id,
                    title = user.staff.title,
                    firstName = user.staff.firstName,
                    lastName = user.staff.lastName,
                    otherNames = user.staff.otherNames,
                    email = user.staff.email,
                    phoneNumber = user.staff.phone,
                    staffIdentificationNumber = user.staff.staffIdentificationNumber,
                    profilePictureUrl = user.staff.staffIdentificationNumber
                };
                
                var response = new UserLoginResponse
                {
                    Id = user.Id,
                    createdAt = user.createdAt,
                    updatedAt = user.updatedAt,
                    email = user.email,
                    emailVerifiedAt = user.emailVerifiedAt,
                    lastSeen = user.lastSeen,
                    isAccountActive = user.isAccountActive,
                    hasResetPassword = user.hasResetPassword,
                    staff = staffLoginResponse,
                    role = user.role
                };
                
                response.accessToken = _jwtProvider.GenerateAccessToken(response.Id, AuthorizationDecisionType.HRMUser);
                await _dbContext.UserHasOTP.Where(x => x.email == hasOTP.email).ExecuteDeleteAsync();

                return Shared.Result.Success(response);
            }
        }
    }
}

public class MappConfirmTwoFactorOtpEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/user/two-factor-confirmation", async (ConfirmTwoFactorOptRequest request, ISender sender) =>
        {
            var result = await sender.Send(request);

            if (result.IsFailure)
            {
                return Results.UnprocessableEntity(result.Error);
            };

            if (result.IsSuccess)
            {
                return Results.Ok(result.Value);
            }
            return Results.BadRequest();
        })
         .WithMetadata(new ProducesResponseTypeAttribute(typeof(UserLoginResponse), StatusCodes.Status200OK))
        .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status422UnprocessableEntity))
        .WithMetadata(new ProducesResponseTypeAttribute(StatusCodes.Status204NoContent))
        .WithTags("Authentication-HRM-User")
        .WithGroupName(SwaggerDoc.SwaggerEndpointDefintions.HRAuthService)
         ;
    }
}
