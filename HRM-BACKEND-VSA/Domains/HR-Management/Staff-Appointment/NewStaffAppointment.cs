using AutoMapper;
using Carter;
using FluentValidation;
using HRM_BACKEND_VSA.Contracts;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Entities.Staff;
using HRM_BACKEND_VSA.Extensions;
using HRM_BACKEND_VSA.Serivices.Mail_Service;
using HRM_BACKEND_VSA.Shared;
using HRM_BACKEND_VSA.Utilities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Domains.HR_Management.Staff_Appointment.NewStaffAppointment;

namespace HRM_BACKEND_VSA.Domains.HR_Management.Staff_Appointment
{
    public static class NewStaffAppointment
    {
        public class StaffAppointmentRequest : IRequest<Shared.Result<Guid>>
        {
            public Guid polymorphicId { get; set; }
            public Guid gradeId { get; set; }
            public string appointmentType { get; set; } = String.Empty;
            public string staffType { get; set; } = String.Empty;
            public string paymentSource { get; set; } = String.Empty;
            public DateOnly? endDate { get; set; }
            public DateOnly notionalDate { get; set; }
            public DateOnly substantiveDate { get; set; }
            public DateOnly? firstAppointmentNotionalDate { get; set; }
            public DateOnly? firstAppointmentSubstantiveDate { get; set; }
            public Guid? firstAppointmentGradeId { get; set; }
            public Guid staffSpecialityId { get; set; }
            public string step { get; set; }
        }

        public class Validator : AbstractValidator<StaffAppointmentRequest>
        {
            public Validator()
            {
                RuleFor(c => c.gradeId).NotEmpty();
                RuleFor(c => c.appointmentType).NotEmpty();
                RuleFor(c => c.notionalDate).NotEmpty();
                RuleFor(c => c.substantiveDate).NotEmpty();
                RuleFor(c => c.staffSpecialityId).NotEmpty();
                RuleFor(c => c.step).NotEmpty();
                RuleFor(c => c.appointmentType).NotEmpty();
                RuleFor(c => c.paymentSource).NotEmpty();
            }
        }

        internal sealed class Handler : IRequestHandler<StaffAppointmentRequest, Shared.Result<Guid>>
        {
            private readonly HRMDBContext _dbContext;
            private readonly HRMStaffDBContext _staffDBContext;
            private readonly IValidator<StaffAppointmentRequest> _validator;
            private readonly MailService _mailService;
            private readonly IMapper _mapper;
            public Handler(IMapper mapper, MailService mailService, HRMDBContext dbContext, HRMStaffDBContext staffDBContext, IValidator<StaffAppointmentRequest> validator)
            {
                _dbContext = dbContext;
                _validator = validator;
                _staffDBContext = staffDBContext;
                _mailService = mailService;
                _mapper = mapper;
            }

            public async Task<Result<Guid>> Handle(StaffAppointmentRequest request, CancellationToken cancellationToken)
            {

                var validationResult = await _validator.ValidateAsync(request);

                if (!validationResult.IsValid)
                {
                    return Shared.Result.Failure<Guid>(Error.ValidationError(validationResult));
                }

                var paymentSourceInitial = PaymentSourceResponseInitials.getGetInitialsFromStaffRequestType(request.paymentSource);
                if (paymentSourceInitial is null) return Shared.Result.Failure<Guid>(Error.BadRequest("Failed to Process Payment Source"));

                var staffType = StaffTypesInitials.getGetInitialsFromStaffRequestType(request.staffType);
                if (staffType is null) return Shared.Result.Failure<Guid>(Error.BadRequest("Failed to Process Type of Staff"));

                var currentNumberOfStaff = await _dbContext.Staff.CountAsync();
                var currentStaffNumberInitials = currentNumberOfStaff > 0 ? currentNumberOfStaff : 1;
                var staffcspsInitials = $"{staffType}{paymentSourceInitial}";

                var newStaffID = Stringutilities.GenerateStaffNumberPerRegistratonCriteria(staffcspsInitials, currentStaffNumberInitials);

                var staffApplicationData = await _staffDBContext
                    .ApplicantBioData
                    .FirstOrDefaultAsync(x => x.applicantId == request.polymorphicId);

                if (staffApplicationData is null)
                {
                    return Shared.Result.Failure<Guid>(Error.CreateNotFoundError("Applicant Bio Data Not Found"));
                }

                var staffRequest = await _dbContext.StaffRequest.FirstOrDefaultAsync(r => r.requestType == RegisterationRequestTypes.newRegisteration && r.RequestDetailPolymorphicId == request.polymorphicId);

                if (staffRequest == null)
                {
                    return Shared.Result.Failure<Guid>(Error.CreateNotFoundError("Request Data Not Found"));
                }
                using (var transaction = await _dbContext.Database.BeginTransactionAsync())
                {

                    try
                    {

                        // Check if existing staff has been created using same request polymorphic id
                        var existingStaffData = await _dbContext.Staff.FirstOrDefaultAsync(x => x.email == staffApplicationData.email);

                        if (existingStaffData is not null)
                        {
                            return Shared.Result.Failure<Guid>(Error.BadRequest("Staff record already exist"));
                        }

                        // creating new staff record from staff application data
                        var newStaff = new Staff
                        {
                            staffIdentificationNumber = newStaffID,
                            firstName = staffApplicationData.firstName,
                            lastName = staffApplicationData.surName,
                            otherNames = staffApplicationData.otherNames,
                            dateOfBirth = staffApplicationData.dateOfBirth,
                            phone = staffApplicationData.phoneOne,
                            gender = staffApplicationData.gender,
                            SNNITNumber = staffApplicationData?.SNNITNumber,
                            GPSAddress = staffApplicationData.GPSAddress,
                            title = staffApplicationData.title,
                            email = staffApplicationData?.email,
                            disability = staffApplicationData?.disability,
                            passportPicture = staffApplicationData?.passportPicture,
                            ECOWASCardNumber = staffApplicationData.ECOWASCardNumber,
                            password = BCrypt.Net.BCrypt.HashPassword(staffApplicationData?.firstName),
                            createdAt = DateTime.UtcNow,
                            updatedAt = DateTime.UtcNow,
                            isAlterable = true,
                            isApproved = true
                        };

                        await _dbContext.Staff.UpdateOrCreate(_dbContext, existingStaffData?.Id, newStaff);

                        //creating first appointmentDetail
                        var firstAppointmentDetail = new StaffAppointment
                        {
                            staffId = newStaff.Id,
                            appointmentType = request.appointmentType,
                            notionalDate = request?.firstAppointmentNotionalDate ?? request.notionalDate,
                            substantiveDate = request?.firstAppointmentSubstantiveDate ?? request.substantiveDate,
                            staffType = request.staffType,
                            staffSpecialityId = request.staffSpecialityId,
                            paymentSource = request.paymentSource,
                            endDate = request.endDate,
                            step = request.step,
                            gradeId = request?.firstAppointmentGradeId ?? request.gradeId
                        };

                        // creating current appointment record
                        var currentAppointmentDetail = new StaffAppointment
                        {
                            staffId = newStaff.Id,
                            appointmentType = request.appointmentType,
                            notionalDate = request.notionalDate,
                            staffSpecialityId = request.staffSpecialityId,
                            substantiveDate = request.substantiveDate,
                            staffType = request.staffType,
                            paymentSource = request.paymentSource,
                            endDate = request.endDate,
                            step = request.step,
                            gradeId = request.gradeId
                        };

                        // Updating staff request status
                        staffRequest.status = StaffRequestStatusTypes.appointed;
                        staffRequest.requestFromStaffId = newStaff.Id;

                        //Update BioDataRequestStatus
                        _dbContext.StaffAppointment.AddRange(new List<StaffAppointment> { firstAppointmentDetail, currentAppointmentDetail });

                        // creating new staff appointment history record
                        var newAppointmentHistoryRecord = _mapper.Map<StaffAppointmentHistory>(currentAppointmentDetail);

                        var staffAppointmentHistory = _dbContext.StaffAppointmentHistory.Add(newAppointmentHistoryRecord);

                        await _dbContext.SaveChangesAsync(cancellationToken);
                        await transaction.CommitAsync(cancellationToken);
                        return Shared.Result.Success<Guid>(newStaff.Id);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Shared.Result.Failure<Guid>(Error.BadRequest(ex.Message));
                    }
                }
            }
        }
    }
}


public class MapNewStaffAppointmentEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/staff-request/new-appointment", async (ISender sender, StaffAppointmentRequest request) =>
        {
            var response = await sender.Send(request);

            if (response.IsSuccess)
            {
                return Results.Ok(response.Value);
            }

            if (response.IsFailure)
            {
                return Results.UnprocessableEntity(response.Error);
            }
            return Results.BadRequest();
        }).WithTags("Staff-Request")
        .WithGroupName(SwaggerDoc.SwaggerEndpointDefintions.AppoinmentAndSeparation)
            ;
    }
}