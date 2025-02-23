using Carter;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Entities.Applicant;
using HRM_BACKEND_VSA.Extensions;
using HRM_BACKEND_VSA.Serivices.Mail_Service;
using HRM_BACKEND_VSA.Services.SMS_Service;
using HRM_BACKEND_VSA.Shared;
using HRM_BACKEND_VSA.Utilities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Features.Applicant.GenerateApplicantOTP;

namespace HRM_BACKEND_VSA.Features.Applicant
{
    public static class GenerateApplicantOTP
    {

        public class GenerateOTPRequest : IRequest<Shared.Result>
        {
            public string contact { get; set; }
        }

        internal sealed class Handler : IRequestHandler<GenerateOTPRequest, Shared.Result>
        {
            private readonly HRMStaffDBContext _context;
            private readonly SMSService _smsService;
            private readonly MailService _mailService;
            public Handler(HRMStaffDBContext dbContext, SMSService smsService, MailService mailService)
            {
                _context = dbContext;
                _smsService = smsService;
                _mailService = mailService;
            }

            public async Task<Shared.Result> Handle(GenerateOTPRequest request, CancellationToken cancellationToken)
            {
                var contact = request.contact;
                //Checking if user hasApplication
                var applicant = await _context.Applicant.FirstOrDefaultAsync(applicant => applicant.contact == contact);
                if (applicant == null) { return Shared.Result.Failure(Error.BadRequest("Your application was not found")); }

                //Checking if application has expired
                DateTime applicationCreationDate = applicant.createdAt;
                bool hasExpired = DateTime.UtcNow.Date - applicationCreationDate.Date > TimeSpan.FromDays(30);
                if (hasExpired) { return Shared.Result.Failure(Error.BadRequest("Application expired")); }

                var otp = Stringutilities.GenerateRandomOtp();

                //Checking if applicant has been sent otp earlier
                var contactOtp = await _context.ApplicantHasOTP.FirstOrDefaultAsync(applicant => applicant.contact == contact);

                if (contactOtp != null)
                {
                    contactOtp.updatedAt = DateTime.UtcNow;
                    contactOtp.otp = otp;
                }
                else
                {
                    await _context.ApplicantHasOTP.AddAsync(new ApplicantHasOTP
                    {
                        otp = otp,
                        applicant = applicant,
                        applicantID = applicant.Id,
                        contact = contact,
                        createdAt = DateTime.UtcNow,
                        updatedAt = DateTime.UtcNow,
                    });
                }
                await _context.SaveChangesAsync();
                var message = SMSMessages.OTPMessage(otp, 10);

                _smsService.SendSMS(contact, message);

                _mailService.SendMail(new Contracts.EmailDTO
                {
                    Subject = "OTP",
                    ToName = $@"{applicant.firsName} {applicant?.lastName}",
                    Body = message,
                    ToEmail = applicant?.email
                });

                return Shared.Result.Success();
            }
        }
    }
}

public class MappGenerateApplicantOTPEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/applicant/generate-otp/{contact}", async (string contact, ISender sender) =>
        {
            var result = await sender.Send(new GenerateOTPRequest
            {
                contact = contact
            });

            if (result.IsFailure)
            {
                return Results.UnprocessableEntity(result.Error);
            };

            return Results.NoContent();
        })
        .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status422UnprocessableEntity))
        .WithMetadata(new ProducesResponseTypeAttribute(StatusCodes.Status204NoContent))
        .WithTags("Authentication Applicant")
        .WithGroupName(SwaggerDoc.SwaggerEndpointDefintions.ApplicantService)
        ;
    }
}
