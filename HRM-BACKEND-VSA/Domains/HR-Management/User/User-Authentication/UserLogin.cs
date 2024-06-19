using AutoMapper;
using Carter;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Serivices.Mail_Service;
using HRM_BACKEND_VSA.Services.SMS_Service;
using HRM_BACKEND_VSA.Shared;
using HRM_BACKEND_VSA.Utilities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Domains.HR_Management.User.User_Authentication.UserLogin;

namespace HRM_BACKEND_VSA.Domains.HR_Management.User.User_Authentication
{
    public class UserLogin
    {

        public class UserLoginRequest : IRequest<Shared.Result<string>>
        {
            public string email { get; set; }
            public string password { get; set; }
        }

        internal sealed class Handler : IRequestHandler<UserLoginRequest, Shared.Result<string>>
        {
            private readonly HRMDBContext _dbContext;
            private readonly IMapper _mapper;
            private readonly SMSService _smsService;
            private readonly MailService _mailService;
            public Handler(HRMDBContext dbContext, IMapper mapper, SMSService smsService, MailService mailService)
            {
                _dbContext = dbContext;
                _mapper = mapper;
                _smsService = smsService;
                _mailService = mailService;
            }

            public async Task<Result<string>> Handle(UserLoginRequest request, CancellationToken cancellationToken)
            {
                var userEmail = request.email;
                //Checking if user Exist
                var user = await _dbContext.User.FirstOrDefaultAsync(s => s.email == userEmail);

                if (user == null) { return Shared.Result.Failure<string>(Error.BadRequest("Invalid Email or Password")); }

                if (BCrypt.Net.BCrypt.Verify(request.password, user.password) is not true)
                {
                    return Shared.Result.Failure<string>(Error.BadRequest("Invalid Email or Password"));
                }

                var otp = Stringutilities.GenerateRandomOtp();

                var userHasOpt = await _dbContext.UserHasOTP.FirstOrDefaultAsync(s => s.email == user.email);

                if (userHasOpt != null)
                {
                    userHasOpt.updatedAt = DateTime.UtcNow;
                    userHasOpt.otp = otp;
                }
                else
                {
                    _dbContext.UserHasOTP.Add(new Entities.HR_Manag.UserHasOTP
                    {
                        email = user.email,
                        otp = otp,
                        createdAt = DateTime.UtcNow,
                        updatedAt = DateTime.UtcNow,
                        userId = user.Id
                    });
                }

                await _dbContext.SaveChangesAsync();

                var message = SMSMessages.OTPMessage(otp, 10);

                _mailService.SendMail(new Contracts.EmailDTO
                {
                    Subject = "OTP",
                    ToName = $@"{user.staff?.firstName} {user.staff?.lastName}",
                    Body = message,
                    ToEmail = user.email
                });

                return Shared.Result.Success($"An OTP has been sent to {user.email}");

            }
        }
    }
}

public class MappUserLoginEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/user/login", async (UserLoginRequest request, ISender sender) =>
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
         .WithMetadata(new ProducesResponseTypeAttribute(StatusCodes.Status200OK))
        .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status422UnprocessableEntity))
        .WithMetadata(new ProducesResponseTypeAttribute(StatusCodes.Status400BadRequest))
        .WithTags("Authentication-HRM-User");
    }
}
