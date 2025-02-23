using Carter;
using FluentValidation;
using HRM_BACKEND_VSA.Contracts;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Domains.HR_Management.Staff_Request.StaffRequestHandlers;
using HRM_BACKEND_VSA.Domains.Staffs.Services;
using HRM_BACKEND_VSA.Domains.Staffs.StaffRequestHandlers;
using HRM_BACKEND_VSA.Entities.HR_Manag;
using HRM_BACKEND_VSA.Entities.Notification.Professional_Licence_Notification;
using HRM_BACKEND_VSA.Entities.Staff;
using HRM_BACKEND_VSA.Extensions;
using HRM_BACKEND_VSA.Providers;
using HRM_BACKEND_VSA.Serivices.Notification_Service;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Domains.HR_Management.StaffRequestHandlers.NewProfessionalLicence;

namespace HRM_BACKEND_VSA.Domains.HR_Management.StaffRequestHandlers
{
    public class ProfessionalLincenseRequest : IStaffRequest
    {
        private readonly HRMDBContext _dbContext;
        private readonly Authprovider _authProvider;
        public ProfessionalLincenseRequest(HRMDBContext dbContext, Authprovider authProvider)
        {
            _dbContext = dbContext;
            _authProvider = authProvider;
        }
        public async Task<object> GetStaffRequestData(Guid RequestDetailPolymorphicId)
        {
            var requestData = await _dbContext.StaffProfessionalLincense.FirstOrDefaultAsync(x => x.Id == RequestDetailPolymorphicId);

            if (requestData is null)
            {
                return Shared.Result.Failure<Staff>(Error.CreateNotFoundError("Registeration Data Not Found"));
            }
            return Shared.Result.Success(requestData);
        }

        public async Task<Guid> NewStaffRequest(Guid RequestDetailPolymorphicId)
        {
            var assignedUser = await new StaffRequestAssignmentService(_dbContext)
                    .EligibleUserEntityForAssingment(RegisterationRequestTypes.bankUpdate);

            if (assignedUser is null)
            {
                throw new Exception("Failed To Assigned Request To Staff");
            }

            var authStaff = await _authProvider.GetAuthStaff();

            var newRegisterationData = new StaffRequest
            {
                createdAt = DateTime.UtcNow,
                updatedAt = DateTime.UtcNow,
                requestType = RegisterationRequestTypes.professionalLicense,
                status = StaffRequestStatusTypes.pending,
                requestFromStaffId = authStaff?.Id,
                RequestDetailPolymorphicId = RequestDetailPolymorphicId,
                requestAssignedStaffId = assignedUser.staffId
            };

            _dbContext.StaffRequest.Add(newRegisterationData);

            var requestNotification = new NewProfessionalLicenceRequestNotfication
            {
                author = authStaff,
                requestDate = DateTime.UtcNow,
                description = "",
                request = newRegisterationData
            };

            try
            {
                await _dbContext.SaveChangesAsync();
                await new Notify<NewProfessionalLicenceRequestNotfication>(_dbContext)
                .dispatch(new NotificationRecord<NewProfessionalLicenceRequestNotfication>(requestNotification, typeof(Entities.User).Name, assignedUser.Id));
            }
            catch (Exception ex)
            {
                throw;
            }
            return newRegisterationData.Id;
        }

        public async Task<string> OnRequestAccepted(Guid RequestDetailPolymorphicId, StaffRequest requestObject)
        {
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var dbRecord = await _dbContext.StaffProfessionalLincense.IgnoreAutoIncludes().Include(sa => sa.staff).FirstOrDefaultAsync(r => r.Id == RequestDetailPolymorphicId);
                    if (dbRecord is null)
                    {
                        throw new Exception("Record was not found");
                    }
                    dbRecord.isApproved = true;
                    dbRecord.isAlterable = true;

                    _dbContext.StaffProfessionalLincenseUpdateHistory.Add(new StaffProfessionalLincenseUpdateHistory
                    {
                        createdAt = DateTime.UtcNow,
                        updatedAt = DateTime.UtcNow,
                        staffId = dbRecord.staffId,
                        professionalBodyId = dbRecord.professionalBodyId,
                        pin = dbRecord.pin,
                        issuedDate = dbRecord.issuedDate,
                        expiryDate = dbRecord.expiryDate,
                        isApproved = true
                    });

                    // Handling request record status update
                    await _dbContext.StaffRequest.Where(x => x.Id == requestObject.Id)
                        .ExecuteUpdateAsync(r =>
                        r.SetProperty(p => p.updatedAt, DateTime.UtcNow)
                        .SetProperty(p => p.status, StaffRequestStatusTypes.approved)
                    );

                    await _dbContext.SaveChangesAsync();

                    var authUser = await _authProvider.GetAuthUser();
                    //Creating Notification Object
                    var acceptionNotifcation = new ProfessionalLicenceAcceptNotification
                    {
                        acceptedOn = DateTime.UtcNow,
                        acceptedBy = authUser,
                        author = dbRecord.staff,
                        description = String.Empty
                    };

                    var notificationObject = new NotificationRecord<ProfessionalLicenceAcceptNotification>(acceptionNotifcation, typeof(Staff).Name, dbRecord.staff.Id);
                    await new Notify<ProfessionalLicenceAcceptNotification>(_dbContext).dispatch(notificationObject);

                    await transaction.CommitAsync();
                    return "Staff Professional Licence Request Approved";
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task OnRequestRejected(Guid RequestDetailPolymorphicId, string? query, StaffRequest requestObject)
        {
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var requestRecord = await _dbContext.StaffProfessionalLincense.Include(ar => ar.staff).FirstOrDefaultAsync(r => r.Id == RequestDetailPolymorphicId);

                    if (requestRecord is null)
                    {
                        throw new Exception("Record was not found");
                    }
                    requestRecord.updatedAt = DateTime.UtcNow;
                    requestRecord.isApproved = false;
                    requestRecord.isAlterable = true;

                    var authUser = await _authProvider.GetAuthUser();

                    var rejectionNotification = new ProfessionalLicenceRejectNotifcation
                    {
                        rejectedOn = DateTime.UtcNow,
                        rejectedBy = authUser,
                        author = requestRecord.staff,
                        description = query
                    };

                    // Handling request record status update
                    await _dbContext.StaffRequest.Where(x => x.Id == requestObject.Id)
                        .ExecuteUpdateAsync(r =>
                        r.SetProperty(p => p.updatedAt, DateTime.UtcNow)
                        .SetProperty(p => p.status, StaffRequestStatusTypes.rejected)
                    );
                    await _dbContext.SaveChangesAsync();

                    var notificationObject = new NotificationRecord<ProfessionalLicenceRejectNotifcation>(rejectionNotification, typeof(Staff).Name, requestRecord.staff.Id);
                    await new Notify<ProfessionalLicenceRejectNotifcation>(_dbContext).dispatch(notificationObject);
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw;

                }
            }
        }
    }


    public static class NewProfessionalLicence
    {
        public class ProfessionalLIncenceRequest : IRequest<Shared.Result<StaffProfessionalLincense>>
        {
            public Guid professionalBodyId { get; set; }
            public string pin { get; set; }
            public DateOnly issuedDate { get; set; }
            public DateOnly expiryDate { get; set; }
        }

        public class Validator : AbstractValidator<ProfessionalLIncenceRequest>
        {
            public Validator()
            {
                RuleFor(c => c.issuedDate).NotEmpty();
                RuleFor(c => c.pin).NotEmpty();
                RuleFor(c => c.professionalBodyId).NotEmpty();
                RuleFor(c => c.expiryDate).NotEmpty();
            }
        }

        internal sealed class Handler : IRequestHandler<ProfessionalLIncenceRequest, HRM_BACKEND_VSA.Shared.Result<StaffProfessionalLincense>>
        {
            private readonly HRMDBContext _dbContext;
            private readonly HRMStaffDBContext _staffDbContext;
            private readonly Authprovider _authProvider;
            private readonly IValidator<ProfessionalLIncenceRequest> _validator;
            private readonly RequestService _requestService;
            public Handler(IValidator<ProfessionalLIncenceRequest> validator, Authprovider authProvider, HRMDBContext dbContext, HRMStaffDBContext staffDbContext, RequestService requestService)
            {
                _validator = validator;
                _authProvider = authProvider;
                _dbContext = dbContext;
                _staffDbContext = staffDbContext;
                _requestService = requestService;
            }
            public async Task<HRM_BACKEND_VSA.Shared.Result<StaffProfessionalLincense>> Handle(ProfessionalLIncenceRequest request, CancellationToken cancellationToken)
            {

                var validationResult = _validator.Validate(request);

                if (validationResult.IsValid is false)
                {
                    return HRM_BACKEND_VSA.Shared.Result.Failure<StaffProfessionalLincense>(HRM_BACKEND_VSA.Shared.Error.ValidationError(validationResult));
                }

                var authStaff = await _authProvider.GetAuthStaff();
                if (authStaff == null) { return HRM_BACKEND_VSA.Shared.Result.Failure<StaffProfessionalLincense>(Error.NotFound); }

                var currentDbRecord = await _dbContext.StaffProfessionalLincense.FirstOrDefaultAsync(sa => sa.staffId == authStaff.Id);

                if (currentDbRecord is not null)
                {
                    if (currentDbRecord.isApproved is false)
                    {
                        return Shared.Result.Failure<StaffProfessionalLincense>(Error.BadRequest("You Have An Unprocessed Bank Request"));
                    }
                }

                using (var dbTransaction = await _dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var newEntry = await _dbContext.StaffProfessionalLincense
                             .UpdateOrCreate(_dbContext, currentDbRecord?.Id, new StaffProfessionalLincense
                             {
                                 updatedAt = DateTime.UtcNow,
                                 staffId = authStaff.Id,
                                 pin = request.pin,
                                 issuedDate = request.issuedDate,
                                 expiryDate = request.expiryDate,
                                 professionalBodyId = request.professionalBodyId,
                                 isApproved = false,
                                 isAlterable = false
                             });

                        var requestService = _requestService.getRequestService(RegisterationRequestTypes.familyDetails);

                        if (requestService is null)
                        {
                            return HRM_BACKEND_VSA.Shared.Result.Failure<StaffProfessionalLincense>(Error.BadRequest("Failed To Resolve Required Request Service"));
                        }

                        try
                        {
                            await requestService.NewStaffRequest(newEntry.Id);
                        }
                        catch (Exception ex)
                        {
                            throw;
                        }

                        await _dbContext.SaveChangesAsync();
                        await dbTransaction.CommitAsync();

                        return Shared.Result.Success(newEntry);
                    }
                    catch (Exception ex)
                    {
                        await dbTransaction.RollbackAsync();
                        return Shared.Result.Failure<StaffProfessionalLincense>(Error.BadRequest(ex.Message));
                    }

                }
            }
        }
    }
}

public class MapNewProfessionalLicenceEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/staff-request/professional-licence",
        [Authorize(Policy = AuthorizationDecisionType.Staff)]
        async (ISender sender, ProfessionalLIncenceRequest request) =>
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

            return Results.BadRequest("Something Went Wrong");

        }).WithTags("Staff-Request")
            .WithGroupName(SwaggerDoc.SwaggerEndpointDefintions.StaffRequestHandler)
            ;
    }
}