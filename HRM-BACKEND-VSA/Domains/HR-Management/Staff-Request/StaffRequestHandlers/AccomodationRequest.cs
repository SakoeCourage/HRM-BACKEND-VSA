using AutoMapper;
using Carter;
using FluentValidation;
using HRM_BACKEND_VSA.Contracts;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Domains.HR_Management.Staff_Request.StaffRequestHandlers;
using HRM_BACKEND_VSA.Domains.Staffs.Services;
using HRM_BACKEND_VSA.Domains.Staffs.StaffRequestHandlers;
using HRM_BACKEND_VSA.Entities.HR_Manag;
using HRM_BACKEND_VSA.Entities.Notification.Accomodation_Request_Notification;
using HRM_BACKEND_VSA.Entities.Staff;
using HRM_BACKEND_VSA.Extensions;
using HRM_BACKEND_VSA.Providers;
using HRM_BACKEND_VSA.Serivices.Notification_Service;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Domains.HR_Management.StaffRequestHandlers.newAccomodationRequest;

namespace HRM_BACKEND_VSA.Domains.HR_Management.StaffRequestHandlers
{
    public class AccomodationRequest(HRMDBContext _dbContext, IMapper _mapper, Authprovider authProvider) : IStaffRequest
    {

        public async Task<object> GetStaffRequestData(Guid RequestDetailPolymorphicId)
        {
            var requestData = await _dbContext.StaffAccomodationDetail.FirstOrDefaultAsync(x => x.Id == RequestDetailPolymorphicId);
            return requestData;
        }

        public async Task<Guid> NewStaffRequest(Guid RequestDetailPolymorphicId)
        {
            var assignedUser = await new StaffRequestAssignmentService(_dbContext)
                .EligibleUserEntityForAssingment(RegisterationRequestTypes.accomodation);

            if (assignedUser is null)
            {
                throw new Exception("Failed To Assigned Request To Staff");
            }
            var authStaff = await authProvider.GetAuthStaff();

            var newRegisterationData = new StaffRequest
            {
                createdAt = DateTime.UtcNow,
                updatedAt = DateTime.UtcNow,
                requestType = RegisterationRequestTypes.accomodation,
                status = StaffRequestStatusTypes.pending,
                requestFromStaffId = authStaff?.Id,
                RequestDetailPolymorphicId = RequestDetailPolymorphicId,
                requestAssignedStaffId = assignedUser.staffId
            };
            _dbContext.StaffRequest.Add(newRegisterationData);


            var accomodationNotificationData = new NewAccomodationRequestNotification
            {
                author = authStaff,
                requestDate = DateTime.UtcNow,
                description = "",
                request = newRegisterationData
            };

            try
            {
                await _dbContext.SaveChangesAsync();
                await new Notify<NewAccomodationRequestNotification>(_dbContext)
                    .dispatch(
                    new NotificationRecord<NewAccomodationRequestNotification>(accomodationNotificationData, typeof(Entities.User).Name, assignedUser.Id)
                    );
            }



            catch (Exception ex)
            {
                throw;
            }
            return newRegisterationData.Id;
        }



        // Handle on Request Accepted
        public async Task<string> OnRequestAccepted(Guid RequestDetailPolymorphicId, StaffRequest requestRecord)
        {
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var accomodationRequest = await _dbContext.StaffAccomodationDetail.IgnoreAutoIncludes().Include(sa => sa.staff).FirstOrDefaultAsync(r => r.Id == RequestDetailPolymorphicId);
                    if (accomodationRequest is null)
                    {
                        throw new Exception("Record was not found");
                    }
                    accomodationRequest.isApproved = true;
                    accomodationRequest.isAlterable = true;

                    _dbContext.StaffAccomodationUpdateHistory.Add(new StaffAccomodationUpdateHistory
                    {
                        createdAt = DateTime.UtcNow,
                        updatedAt = DateTime.UtcNow,
                        staffId = accomodationRequest.staffId,
                        source = accomodationRequest.source,
                        gpsAddress = accomodationRequest.gpsAddress,
                        accomodationType = accomodationRequest.accomodationType,
                        allocationDate = accomodationRequest.allocationDate,
                        flatNumber = accomodationRequest.flatNumber,
                        isApproved = true
                    });

                    // Handling request record status update
                    await _dbContext.StaffRequest.Where(x => x.Id == requestRecord.Id)
                        .ExecuteUpdateAsync(r =>
                        r.SetProperty(p => p.updatedAt, DateTime.UtcNow)
                        .SetProperty(p => p.status, StaffRequestStatusTypes.approved)
                    );


                    await _dbContext.SaveChangesAsync();

                    var authUser = await authProvider.GetAuthUser();
                    //Creating Notification Object
                    var acceptionNotifcation = new AccomdationAcceptNotification
                    {
                        acceptedOn = DateTime.UtcNow,
                        acceptedBy = authUser,
                        author = accomodationRequest.staff,
                        description = String.Empty
                    };
                    var notificationObject = new NotificationRecord<AccomdationAcceptNotification>(acceptionNotifcation, typeof(Staff).Name, accomodationRequest.staff.Id);

                    await new Notify<AccomdationAcceptNotification>(_dbContext).dispatch(notificationObject);
                    await transaction.CommitAsync();
                    return "Staff Accomodation Request Approved";
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        // Handle on Request Rejected
        public async Task OnRequestRejected(Guid RequestDetailPolymorphicId, string? query, StaffRequest requestObject)
        {
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var accomodationDataRequest = await _dbContext.StaffAccomodationDetail.Include(ar => ar.staff).FirstOrDefaultAsync(r => r.Id == RequestDetailPolymorphicId);

                    if (accomodationDataRequest is null)
                    {
                        throw new Exception("Record was not found");
                    }
                    accomodationDataRequest.updatedAt = DateTime.UtcNow;
                    accomodationDataRequest.isApproved = false;
                    accomodationDataRequest.isAlterable = true;

                    var authUser = await authProvider.GetAuthUser();

                    var rejectionNotification = new AccomodationRejectNotification
                    {
                        rejectedOn = DateTime.UtcNow,
                        rejectedBy = authUser,
                        author = accomodationDataRequest.staff,
                        description = query
                    };

                    // Handling request record status update
                    await _dbContext.StaffRequest.Where(x => x.Id == requestObject.Id)
                        .ExecuteUpdateAsync(r =>
                        r.SetProperty(p => p.updatedAt, DateTime.UtcNow)
                        .SetProperty(p => p.status, StaffRequestStatusTypes.rejected)
                    );
                    await _dbContext.SaveChangesAsync();

                    var notificationObject = new NotificationRecord<AccomodationRejectNotification>(rejectionNotification, typeof(Staff).Name, accomodationDataRequest.staff.Id);
                    await new Notify<AccomodationRejectNotification>(_dbContext).dispatch(notificationObject);
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw ex;

                }
            }




        }
    }




    //Handle New Staff Accomodation
    public static class newAccomodationRequest
    {
        public class NewAccomodationRequestData : IRequest<Shared.Result<StaffAccomodationDetail>>
        {
            public string source { get; set; } = String.Empty;
            public string gpsAddress { get; set; } = String.Empty;
            public string accomodationType { get; set; } = String.Empty;
            public string flatNumber { get; set; } = String.Empty;
            public DateOnly? allocationDate { get; set; }
        }

        public class validator : AbstractValidator<NewAccomodationRequestData>
        {
            public validator()
            {
                RuleFor(c => c.source).NotEmpty();
                RuleFor(c => c.gpsAddress).NotEmpty();
                RuleFor(c => c.accomodationType).NotEmpty();
            }

        }

        internal sealed class Handler : IRequestHandler<NewAccomodationRequestData, Shared.Result<StaffAccomodationDetail>>
        {
            private readonly HRMDBContext _dbContext;
            private readonly HRMStaffDBContext _staffDbContext;
            private readonly Authprovider _authProvider;
            private readonly IValidator<NewAccomodationRequestData> _validator;
            private readonly RequestService _requestService;
            public Handler(IValidator<NewAccomodationRequestData> validator, Authprovider authProvider, HRMDBContext dbContext, HRMStaffDBContext staffDbContext, RequestService requestService)
            {
                _validator = validator;
                _authProvider = authProvider;
                _dbContext = dbContext;
                _staffDbContext = staffDbContext;
                _requestService = requestService;
            }
            public async Task<Shared.Result<StaffAccomodationDetail>> Handle(NewAccomodationRequestData request, CancellationToken cancellationToken)
            {
                var authStaff = await _authProvider.GetAuthStaff();
                if (authStaff == null) { return Shared.Result.Failure<StaffAccomodationDetail>(Error.NotFound); };

                var currentAccomodationData = await _dbContext.StaffAccomodationDetail.FirstOrDefaultAsync(sa => sa.staffId == authStaff.Id);

                if (currentAccomodationData is not null)
                {
                    if (currentAccomodationData.isApproved is false)
                    {
                        return Shared.Result.Failure<StaffAccomodationDetail>(Error.BadRequest("You Have Unprocessed Accomodation Request"));
                    }
                }


                using (var dbTransaction = await _dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var newEntry = await _dbContext.StaffAccomodationDetail
                             .UpdateOrCreate(_dbContext, currentAccomodationData?.Id ?? null, new StaffAccomodationDetail
                             {
                                 updatedAt = DateTime.UtcNow,
                                 staffId = authStaff.Id,
                                 source = request.source,
                                 gpsAddress = request.gpsAddress,
                                 flatNumber = request.flatNumber,
                                 accomodationType = request.accomodationType,
                                 allocationDate = request.allocationDate,
                                 isApproved = false,
                                 isAlterable = false
                             });

                        var requestService = _requestService.getRequestService(RegisterationRequestTypes.accomodation);

                        if (requestService is null)
                        {
                            return Shared.Result.Failure<StaffAccomodationDetail>(Error.BadRequest("Failed To Resolve Required Request Service"));
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
                        return Shared.Result.Failure<StaffAccomodationDetail>(Error.BadRequest(ex.Message));

                    }

                }
            }
        }

    }
}


public class MappAcomodationRequestEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/staff-request/accommodation",
        [Authorize(Policy = AuthorizationDecisionType.Staff)]
        async (ISender sender, NewAccomodationRequestData request) =>
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
