using Carter;
using FluentValidation;
using HRM_BACKEND_VSA.Contracts;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Domains.HR_Management.Staff_Request.StaffRequestHandlers;
using HRM_BACKEND_VSA.Domains.Staffs.Services;
using HRM_BACKEND_VSA.Domains.Staffs.StaffRequestHandlers;
using HRM_BACKEND_VSA.Entities.HR_Manag;
using HRM_BACKEND_VSA.Entities.Notification.Children_Request_Notification;
using HRM_BACKEND_VSA.Entities.Staff;
using HRM_BACKEND_VSA.Extensions;
using HRM_BACKEND_VSA.Providers;
using HRM_BACKEND_VSA.Serivices.Notification_Service;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Domains.HR_Management.StaffRequestHandlers.NewChildrenDetail;

namespace HRM_BACKEND_VSA.Domains.HR_Management.StaffRequestHandlers
{
    public class ChildrenDetailRequest : IStaffRequest
    {
        private readonly HRMDBContext _dbContext;
        private readonly Authprovider _authProvider;
        public ChildrenDetailRequest(HRMDBContext dbContext, Authprovider authProvider)
        {
            _dbContext = dbContext;
            _authProvider = authProvider;
        }
        public async Task<object> GetStaffRequestData(Guid RequestDetailPolymorphicId)
        {
            var requestData = await _dbContext.StaffChildrenDetail.FirstOrDefaultAsync(x => x.Id == RequestDetailPolymorphicId);
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
                requestType = RegisterationRequestTypes.childrenDetails,
                status = StaffRequestStatusTypes.pending,
                requestFromStaffId = authStaff?.Id,
                RequestDetailPolymorphicId = RequestDetailPolymorphicId,
                requestAssignedStaffId = assignedUser.staffId
            };

            _dbContext.StaffRequest.Add(newRegisterationData);


            var requestNotification = new NewChildrenRequestNotification
            {
                author = authStaff,
                requestDate = DateTime.UtcNow,
                description = "",
                request = newRegisterationData
            };
            try
            {
                await _dbContext.SaveChangesAsync();
                await new Notify<NewChildrenRequestNotification>(_dbContext)
                .dispatch(
                new NotificationRecord<NewChildrenRequestNotification>(requestNotification, typeof(Entities.User).Name, assignedUser.Id)
                );
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
                    var ChildrenDetailRecord = await _dbContext.StaffChildrenDetail.IgnoreAutoIncludes().Include(sa => sa.staff).FirstOrDefaultAsync(r => r.Id == RequestDetailPolymorphicId);
                    if (ChildrenDetailRecord is null)
                    {
                        throw new Exception("Record was not found");
                    }
                    ChildrenDetailRecord.isApproved = true;
                    ChildrenDetailRecord.isAlterable = true;

                    _dbContext.StaffChildrenUpdateHistory.Add(new StaffChildrenUpdateHistory
                    {
                        createdAt = DateTime.UtcNow,
                        updatedAt = DateTime.UtcNow,
                        staffId = ChildrenDetailRecord.staffId,
                        childName = ChildrenDetailRecord.childName,
                        dateOfBirth = ChildrenDetailRecord.dateOfBirth,
                        gender = ChildrenDetailRecord.gender,
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
                    var acceptionNotifcation = new ChildrenAcceptNotification
                    {
                        acceptedOn = DateTime.UtcNow,
                        acceptedBy = authUser,
                        author = ChildrenDetailRecord.staff,
                        description = String.Empty
                    };
                    var notificationObject = new NotificationRecord<ChildrenAcceptNotification>(acceptionNotifcation, typeof(Staff).Name, ChildrenDetailRecord.staff.Id);

                    await new Notify<ChildrenAcceptNotification>(_dbContext).dispatch(notificationObject);
                    await transaction.CommitAsync();
                    return "Staff Child Detail Request Approved";
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
                    var requestRecord = await _dbContext.StaffChildrenDetail.Include(ar => ar.staff).FirstOrDefaultAsync(r => r.Id == RequestDetailPolymorphicId);

                    if (requestRecord is null)
                    {
                        throw new Exception("Record was not found");
                    }
                    requestRecord.updatedAt = DateTime.UtcNow;
                    requestRecord.isApproved = false;
                    requestRecord.isAlterable = true;

                    var authUser = await _authProvider.GetAuthUser();

                    var rejectionNotification = new ChildrenRejectNotification
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

                    var notificationObject = new NotificationRecord<ChildrenRejectNotification>(rejectionNotification, typeof(Staff).Name, requestRecord.staff.Id);
                    await new Notify<ChildrenRejectNotification>(_dbContext).dispatch(notificationObject);
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

    public static class NewChildrenDetail
    {
        public class NewChildrenRequestData : IRequest<Shared.Result<StaffChildrenDetail>>
        {
            public string childName { get; set; } = string.Empty;
            public DateOnly dateOfBirth { get; set; }
            public string gender { get; set; } = string.Empty;
        }

        public class Validator : AbstractValidator<NewChildrenRequestData>
        {
            public Validator()
            {
                RuleFor(c => c.dateOfBirth).NotEmpty();
                RuleFor(c => c.childName).NotEmpty();
                RuleFor(c => c.gender).NotEmpty();
            }
        }

        internal sealed class Handler : IRequestHandler<NewChildrenRequestData, Shared.Result<StaffChildrenDetail>>
        {
            private readonly HRMDBContext _dbContext;
            private readonly HRMStaffDBContext _staffDbContext;
            private readonly Authprovider _authProvider;
            private readonly IValidator<NewChildrenRequestData> _validator;
            private readonly RequestService _requestService;
            public Handler(IValidator<NewChildrenRequestData> validator, Authprovider authProvider, HRMDBContext dbContext, HRMStaffDBContext staffDbContext, RequestService requestService)
            {
                _validator = validator;
                _authProvider = authProvider;
                _dbContext = dbContext;
                _staffDbContext = staffDbContext;
                _requestService = requestService;
            }
            public async Task<Shared.Result<StaffChildrenDetail>> Handle(NewChildrenRequestData request, CancellationToken cancellationToken)
            {

                var validationResult = _validator.Validate(request);

                if (validationResult.IsValid is false)
                {
                    return Shared.Result.Failure<StaffChildrenDetail>(Error.ValidationError(validationResult));
                }

                var authStaff = await _authProvider.GetAuthStaff();
                if (authStaff == null) { return Shared.Result.Failure<StaffChildrenDetail>(Error.NotFound); };

                using (var dbTransaction = await _dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var newEntry = await _dbContext.StaffChildrenDetail
                             .UpdateOrCreate(_dbContext, null, new StaffChildrenDetail
                             {
                                 updatedAt = DateTime.UtcNow,
                                 staffId = authStaff.Id,
                                 childName = request.childName,
                                 dateOfBirth = request.dateOfBirth,
                                 gender = request.gender,
                                 isApproved = false,
                                 isAlterable = false
                             });

                        var requestService = _requestService.getRequestService(RegisterationRequestTypes.bankUpdate);

                        if (requestService is null)
                        {
                            return Shared.Result.Failure<StaffChildrenDetail>(Error.BadRequest("Failed To Resolve Required Request Service"));
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
                        return Shared.Result.Failure<StaffChildrenDetail>(Error.BadRequest(ex.Message));

                    }

                }
            }
        }
    }
}

public class MapNewChildrenDetailEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/staff-request/children-details",
        [Authorize(Policy = AuthorizationDecisionType.Staff)]
        async (ISender sender, NewChildrenRequestData request) =>
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
