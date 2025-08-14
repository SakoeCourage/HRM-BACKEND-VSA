using AutoMapper;
using Carter;
using FluentValidation;
using HRM_BACKEND_VSA.Contracts;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Entities.Staff;
using HRM_BACKEND_VSA.Extensions;
using HRM_BACKEND_VSA.Serivices.Mail_Service;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Contracts.EmailContracts;
using static HRM_BACKEND_VSA.Domains.HR_Management.Staff_Posting.NewStaffPosting;

namespace HRM_BACKEND_VSA.Domains.HR_Management.Staff_Posting
{
    public static class NewStaffPosting
    {
        public class NewStaffPostingRequest : IRequest<Shared.Result<Guid>>
        {
            public Guid? staffId { get; set; }
            public Guid? polymorphicId { get; set; }
            public Guid directorateId { get; set; }
            public Guid departmentId { get; set; }
            public Guid unitId { get; set; }
            public DateOnly postingDate { get; set; }
        }

        public class validator : AbstractValidator<NewStaffPostingRequest>
        {
            public validator()
            {
                RuleFor(c => c.directorateId)
                    .NotEmpty();
                RuleFor(c => c.departmentId)
                    .NotEmpty();
                RuleFor(c => c.unitId)
                    .NotEmpty();
                RuleFor(c => c.postingDate)
                    .NotEmpty();
            }
        }

        internal sealed class Handler : IRequestHandler<NewStaffPostingRequest, Shared.Result<Guid>>
        {
            private readonly HRMDBContext _dbContext;
            private readonly HRMStaffDBContext _staffDBContext;
            private readonly IValidator<NewStaffPostingRequest> _validator;
            private readonly MailService _mailService;
            private readonly IMapper _mapper;

            public Handler(IMapper mapper, MailService mailService, HRMDBContext dbContext,
                HRMStaffDBContext staffDBContext, IValidator<NewStaffPostingRequest> validator)
            {
                _dbContext = dbContext;
                _validator = validator;
                _staffDBContext = staffDBContext;
                _mailService = mailService;
                _mapper = mapper;
            }

            public async Task<Result<Guid>> Handle(NewStaffPostingRequest request, CancellationToken cancellationToken)
            {
                var validationResult = _validator.Validate(request);

                if (request?.polymorphicId == null && request?.staffId == null)
                {
                    return Shared.Result.Failure<Guid>(Error.BadRequest("Polymorphic Id or Staff Id is required"));
                }

                if (validationResult.IsValid is false)
                {
                    return Shared.Result.Failure<Guid>(Error.ValidationError(validationResult));
                }


                var unit = await _dbContext.Unit.Include(u => u.directorate).Include(u => u.department)
                    .FirstOrDefaultAsync(u => u.Id == request.unitId);

                if (unit is null)
                {
                    return Shared.Result.Failure<Guid>(Error.CreateNotFoundError("Unit Data Not Found"));
                }

                using (var dbTransaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken))
                {
                    try
                    {
                        Staff? existingStaff = null;

                        if (request.polymorphicId != null)
                        {
                            var staffRequest = await _dbContext.StaffRequest
                                .Include(r => r.requestFromStaff)
                                .FirstOrDefaultAsync(r =>
                                    r.requestType == RegisterationRequestTypes.newRegisteration &&
                                    r.RequestDetailPolymorphicId == request.polymorphicId);

                            if (staffRequest == null && request.polymorphicId != null)
                            {
                                return Shared.Result.Failure<Guid>(Error.CreateNotFoundError("Request Data Not Found"));
                            }
                            
                            // Updating staff request status
                            staffRequest.status = StaffRequestStatusTypes.posted;

                            existingStaff = await _dbContext.Staff.Include(s => s.currentAppointment)
                                .FirstOrDefaultAsync(s => s.Id == staffRequest.requestFromStaff.Id);
                        }
                        else
                        {
                            existingStaff = await _dbContext.Staff.Include(st => st.currentAppointment)
                                .FirstOrDefaultAsync(st => st.Id == request.staffId);
                        }
                        
                        if (existingStaff is null)
                        {
                            return Shared.Result.Failure<Guid>(Error.CreateNotFoundError("Staff Data Not Found"));
                        }

                        if (existingStaff?.currentAppointment is null)
                        {
                            return Shared.Result.Failure<Guid>(
                                Error.CreateNotFoundError("Staff Appointment Data Not Found"));
                        }

                        // Creating New Posting Data
                        var newPostingData = new StaffPosting
                        {
                            staffId = existingStaff.Id,
                            departmentId = request.departmentId,
                            unitId = request.unitId,
                            directorateId = request.directorateId,
                            postingDate = request.postingDate,
                            createdAt = DateTime.UtcNow,
                            updatedAt = DateTime.UtcNow
                        };

                        // Adding Posting Data To History
                        var newStaffPostingHistory = new StaffPostingHistory
                        {
                            staffId = existingStaff.Id,
                            departmentId = request.departmentId,
                            unitId = request.unitId,
                            postingDate = request.postingDate,
                            directorateId = request.directorateId,
                            createdAt = DateTime.UtcNow,
                            updatedAt = DateTime.UtcNow
                        };

                        var emailDTO = new EmailDTO
                        {
                            Subject = "Staff Posting",
                            Body = EmailContracts.generateStaffPostingEmailBodyTemplate(new StaffPostingRecord(
                                firstName: existingStaff.firstName,
                                lastName: existingStaff.lastName,
                                staffType: existingStaff.currentAppointment?.staffType ?? "",
                                staffId: existingStaff.staffIdentificationNumber,
                                unitName: unit.unitName,
                                departmentName: unit.department.departmentName,
                                directorateName: unit.directorate.directorateName,
                                notionalDate: existingStaff.currentAppointment.notionalDate
                            )),
                            ToEmail = existingStaff.email,
                            ToName = $"{existingStaff.firstName} {existingStaff.lastName}"
                        };

                        _dbContext.StaffPosting.Add(newPostingData);
                        _dbContext.StaffPostingHistory.Add(newStaffPostingHistory);

                        await _dbContext.SaveChangesAsync();
                        await dbTransaction.CommitAsync();
                        _mailService.SendMail(emailDTO);
                        return Shared.Result.Success<Guid>(newPostingData.Id);
                    }
                    catch (Exception ex)
                    {
                        await dbTransaction.RollbackAsync(cancellationToken);
                        return Shared.Result.Failure<Guid>(Error.BadRequest(ex.Message));
                    }
                }
            }
        }
    }
}

public class MapNewStaffPostingEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/staff-request/new-posting", async (ISender sender, NewStaffPostingRequest request) =>
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
            .WithGroupName(SwaggerDoc.SwaggerEndpointDefintions.PostingAndTransfer)
            ;
    }
}