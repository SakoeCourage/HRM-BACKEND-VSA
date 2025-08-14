using Carter;
using FluentValidation;
using HRM_BACKEND_VSA.Contracts;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Domains.HR_Management.Staff_Request.StaffRequestHandlers;
using HRM_BACKEND_VSA.Domains.Staffs.Services;
using HRM_BACKEND_VSA.Domains.Staffs.StaffRequestHandlers;
using HRM_BACKEND_VSA.Entities.HR_Manag;
using HRM_BACKEND_VSA.Entities.Notification.Family_Request_Notification;
using HRM_BACKEND_VSA.Entities.Staff;
using HRM_BACKEND_VSA.Extensions;
using HRM_BACKEND_VSA.Providers;
using HRM_BACKEND_VSA.Serivices.Notification_Service;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Domains.HR_Management.StaffRequestHandlers.AddFamilyDetail;

namespace HRM_BACKEND_VSA.Domains.HR_Management.StaffRequestHandlers
{
    public class FamilyDetailRequest : IStaffRequest
    {
        private readonly HRMDBContext _dbContext;
        private readonly Authprovider _authProvider;
        public FamilyDetailRequest(HRMDBContext dbContext, Authprovider authProvider)
        {
            _dbContext = dbContext;
            _authProvider = authProvider;
        }
        public async Task<object> GetStaffRequestData(Guid RequestDetailPolymorphicId)
        {
            var requestData = await _dbContext.StaffFamilyDetail.FirstOrDefaultAsync(x => x.Id == RequestDetailPolymorphicId);
            return requestData;
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
                requestType = RegisterationRequestTypes.familyDetails,
                status = StaffRequestStatusTypes.pending,
                requestFromStaffId = authStaff?.Id,
                RequestDetailPolymorphicId = RequestDetailPolymorphicId,
                requestAssignedStaffId = assignedUser.staffId
            };

            _dbContext.StaffRequest.Add(newRegisterationData);

            var requestNotification = new NewFamilyRequestNotification
            {
                author = authStaff,
                requestDate = DateTime.UtcNow,
                description = "",
                request = newRegisterationData
            };
            try
            {
                await _dbContext.SaveChangesAsync();
                await new Notify<NewFamilyRequestNotification>(_dbContext)
                .dispatch(
                new NotificationRecord<NewFamilyRequestNotification>(requestNotification, typeof(Entities.User).Name, assignedUser.Id)
           );
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return newRegisterationData.Id;
        }

        public async Task<string> OnRequestAccepted(Guid RequestDetailPolymorphicId, StaffRequest requestObject)
        {
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var dbRecord = await _dbContext.StaffFamilyDetail.IgnoreAutoIncludes().Include(sa => sa.staff).FirstOrDefaultAsync(r => r.Id == RequestDetailPolymorphicId);
                    if (dbRecord is null)
                    {
                        throw new Exception("Record was not found");
                    }
                    dbRecord.isApproved = true;
                    dbRecord.isAlterable = true;

                    _dbContext.StaffFamilyUpdatetHistory.Add(new StaffFamilyUpdatetHistory
                    {
                        createdAt = DateTime.UtcNow,
                        updatedAt = DateTime.UtcNow,
                        staffId = dbRecord.staffId,
                        emergencyPerson = dbRecord.emergencyPerson,
                        fathersName = dbRecord.fathersName,
                        mothersName = dbRecord.mothersName,
                        spouseName = dbRecord.spouseName,
                        spousePhoneNumber = dbRecord.spousePhoneNumber,
                        nextOfKIN = dbRecord.nextOfKIN,
                        nextOfKINPhoneNumber = dbRecord.nextOfKINPhoneNumber,
                        emergencyPersonPhoneNumber = dbRecord.emergencyPersonPhoneNumber,
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
                    var acceptionNotifcation = new FamilyAcceptNotification
                    {
                        acceptedOn = DateTime.UtcNow,
                        acceptedBy = authUser,
                        author = dbRecord.staff,
                        description = String.Empty
                    };
                    var notificationObject = new NotificationRecord<FamilyAcceptNotification>(acceptionNotifcation, typeof(Staff).Name, dbRecord.staff.Id);

                    await new Notify<FamilyAcceptNotification>(_dbContext).dispatch(notificationObject);
                    await transaction.CommitAsync();
                    return "Staff Family Detail Request Approved";
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
                    var requestRecord = await _dbContext.StaffFamilyDetail.Include(ar => ar.staff).FirstOrDefaultAsync(r => r.Id == RequestDetailPolymorphicId);

                    if (requestRecord is null)
                    {
                        throw new Exception("Record was not found");
                    }
                    requestRecord.updatedAt = DateTime.UtcNow;
                    requestRecord.isApproved = false;
                    requestRecord.isAlterable = true;

                    var authUser = await _authProvider.GetAuthUser();

                    var rejectionNotification = new FamilyRejectNotification
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

                    var notificationObject = new NotificationRecord<FamilyRejectNotification>(rejectionNotification, typeof(Staff).Name, requestRecord.staff.Id);
                    await new Notify<FamilyRejectNotification>(_dbContext).dispatch(notificationObject);
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
    public static class AddFamilyDetail
    {
        public class AddFamilyDetailRequest : IRequest<HRM_BACKEND_VSA.Shared.Result<StaffFamilyDetail>>
        {
            public string fathersName { get; set; } = String.Empty;
            public string mothersName { get; set; } = String.Empty;
            public string spouseName { get; set; } = String.Empty;
            public string spousePhoneNumber { get; set; } = String.Empty;
            public string nextOfKIN { get; set; } = String.Empty;
            public string nextOfKINPhoneNumber { get; set; } = String.Empty;
            public string emergencyPerson { get; set; } = String.Empty;
            public string emergencyPersonPhoneNumber { get; set; } = String.Empty;
        }

        public class Validator : AbstractValidator<AddFamilyDetailRequest>
        {
            public Validator()
            {
                RuleFor(c => c.fathersName).NotEmpty();
                RuleFor(c => c.mothersName).NotEmpty();
                RuleFor(c => c.nextOfKIN).NotEmpty();
                RuleFor(c => c.nextOfKINPhoneNumber).NotEmpty();
                RuleFor(c => c.emergencyPerson).NotEmpty();
                RuleFor(c => c.emergencyPersonPhoneNumber).NotEmpty();
            }
        }

        internal sealed class Handler : IRequestHandler<AddFamilyDetailRequest, HRM_BACKEND_VSA.Shared.Result<StaffFamilyDetail>>
        {
            private readonly HRMDBContext _dbContext;
            private readonly HRMStaffDBContext _staffDbContext;
            private readonly Authprovider _authProvider;
            private readonly IValidator<AddFamilyDetailRequest> _validator;
            private readonly RequestService _requestService;
            public Handler(IValidator<AddFamilyDetailRequest> validator, Authprovider authProvider, HRMDBContext dbContext, HRMStaffDBContext staffDbContext, RequestService requestService)
            {
                _validator = validator;
                _authProvider = authProvider;
                _dbContext = dbContext;
                _staffDbContext = staffDbContext;
                _requestService = requestService;
            }
            public async Task<HRM_BACKEND_VSA.Shared.Result<StaffFamilyDetail>> Handle(AddFamilyDetailRequest request, CancellationToken cancellationToken)
            {

                var validationResult = _validator.Validate(request);

                if (validationResult.IsValid is false)
                {
                    return HRM_BACKEND_VSA.Shared.Result.Failure<StaffFamilyDetail>(HRM_BACKEND_VSA.Shared.Error.ValidationError(validationResult));
                }

                var authStaff = await _authProvider.GetAuthStaff();
                if (authStaff == null) { return HRM_BACKEND_VSA.Shared.Result.Failure<StaffFamilyDetail>(Error.NotFound); }

                var currentDbRecord = await _dbContext.StaffFamilyDetail.FirstOrDefaultAsync(sa => sa.staffId == authStaff.Id);

                if (currentDbRecord is not null)
                {
                    if (currentDbRecord.isApproved is false)
                    {
                        return Shared.Result.Failure<StaffFamilyDetail>(Error.BadRequest("You have a pending request"));
                    }
                }

                using (var dbTransaction = await _dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var newEntry = await _dbContext.StaffFamilyDetail
                             .UpdateOrCreate(_dbContext, currentDbRecord?.Id, new StaffFamilyDetail
                             {
                                 updatedAt = DateTime.UtcNow,
                                 staffId = authStaff.Id,
                                 fathersName = request.fathersName,
                                 mothersName = request.mothersName,
                                 spouseName = request?.spouseName ?? String.Empty,
                                 spousePhoneNumber = request.spousePhoneNumber,
                                 nextOfKIN = request?.nextOfKIN ?? String.Empty,
                                 nextOfKINPhoneNumber = request?.nextOfKINPhoneNumber ?? String.Empty,
                                 emergencyPerson = request.emergencyPerson,
                                 emergencyPersonPhoneNumber = request.emergencyPersonPhoneNumber,
                                 isApproved = false,
                                 isAlterable = false
                             });

                        var requestService = _requestService.getRequestService(RegisterationRequestTypes.familyDetails);

                        if (requestService is null)
                        {
                            return HRM_BACKEND_VSA.Shared.Result.Failure<StaffFamilyDetail>(Error.BadRequest("Failed To Resolve Required Request Service"));
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
                        return Shared.Result.Failure<StaffFamilyDetail>(Error.BadRequest(ex.Message));

                    }

                }
            }
        }

    }
}

public class MapNewFamilyDetailEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/staff-request/family-details",
        [Authorize(Policy = AuthorizationDecisionType.Staff)]
        async (ISender sender, AddFamilyDetailRequest request) =>
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


