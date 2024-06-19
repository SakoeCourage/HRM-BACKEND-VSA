using AutoMapper;
using Carter;
using FluentValidation;
using HRM_BACKEND_VSA.Contracts;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Domains.HR_Management.Staff_Request.StaffRequestHandlers;
using HRM_BACKEND_VSA.Domains.Staffs.Services;
using HRM_BACKEND_VSA.Domains.Staffs.StaffRequestHandlers;
using HRM_BACKEND_VSA.Entities.HR_Manag;
using HRM_BACKEND_VSA.Entities.Notification.BioData_Request_Notification;
using HRM_BACKEND_VSA.Entities.Staff;
using HRM_BACKEND_VSA.Extensions;
using HRM_BACKEND_VSA.Providers;
using HRM_BACKEND_VSA.Serivices.Notification_Service;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Domains.HR_Management.StaffRequestHandlers.NewStaffBioUpdate;

namespace HRM_BACKEND_VSA.Domains.HR_Management.StaffRequestHandlers
{
    public class StaffBioDataRequest : IStaffRequest
    {
        private readonly HRMDBContext _dbContext;
        private readonly Authprovider _authProvider;
        private readonly IMapper _mapper;
        public StaffBioDataRequest(HRMDBContext dbContext, Authprovider authProvider, IMapper mapper)
        {
            _dbContext = dbContext;
            _authProvider = authProvider;
            _mapper = mapper;
        }
        public async Task<object> GetStaffRequestData(Guid RequestDetailPolymorphicId)
        {
            var staffData = await _dbContext.Staff.FirstOrDefaultAsync(x => x.Id == RequestDetailPolymorphicId);

            return staffData;
        }

        public async Task<Guid> NewStaffRequest(Guid RequestDetailPolymorphicId)
        {
            var assignedUser = await new StaffRequestAssignmentService(_dbContext)
                    .EligibleUserEntityForAssingment(RegisterationRequestTypes.bioData);

            if (assignedUser is null)
            {
                throw new Exception("Failed To Assigned Request To Staff");
            }

            var authStaff = await _authProvider.GetAuthStaff();
            var newRegisterationData = new StaffRequest
            {
                createdAt = DateTime.UtcNow,
                updatedAt = DateTime.UtcNow,
                requestType = RegisterationRequestTypes.bioData,
                status = StaffRequestStatusTypes.pending,
                requestFromStaffId = authStaff?.Id,
                RequestDetailPolymorphicId = RequestDetailPolymorphicId,
                requestAssignedStaffId = assignedUser.staffId
            };

            _dbContext.StaffRequest.Add(newRegisterationData);

            var requestNotification = new NewBiodataRequestNotification
            {
                author = authStaff,
                requestDate = DateTime.UtcNow,
                description = "",
                request = newRegisterationData
            };

            try
            {
                await _dbContext.SaveChangesAsync();
                await new Notify<NewBiodataRequestNotification>(_dbContext)
                .dispatch(new NotificationRecord<NewBiodataRequestNotification>(requestNotification, typeof(Entities.User).Name, assignedUser.Id));
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
                    var dbRecord = await _dbContext.Staff.IgnoreAutoIncludes().FirstOrDefaultAsync(r => r.Id == RequestDetailPolymorphicId);

                    if (dbRecord is null)
                    {
                        throw new Exception("Record was not found");
                    }

                    dbRecord.isApproved = true;
                    dbRecord.isAlterable = true;

                    var staffBioUpdateRecord = _mapper.Map<StaffBioUpdateHistory>(dbRecord);
                    staffBioUpdateRecord.isApproved = true;
                    staffBioUpdateRecord.createdAt = DateTime.UtcNow;
                    staffBioUpdateRecord.updatedAt = DateTime.UtcNow;

                    _dbContext.StaffBioUpdateHistory.Add(staffBioUpdateRecord);

                    // Handling request record status update
                    await _dbContext.StaffRequest.Where(x => x.Id == requestObject.Id)
                        .ExecuteUpdateAsync(r =>
                        r.SetProperty(p => p.updatedAt, DateTime.UtcNow)
                        .SetProperty(p => p.status, StaffRequestStatusTypes.approved)
                    );

                    await _dbContext.SaveChangesAsync();

                    var authUser = await _authProvider.GetAuthUser();
                    //Creating Notification Object
                    var acceptionNotifcation = new BiodataAcceptNotification
                    {
                        acceptedOn = DateTime.UtcNow,
                        acceptedBy = authUser,
                        author = dbRecord,
                        description = String.Empty
                    };

                    var notificationObject = new NotificationRecord<BiodataAcceptNotification>(acceptionNotifcation, typeof(Staff).Name, dbRecord.Id);
                    await new Notify<BiodataAcceptNotification>(_dbContext).dispatch(notificationObject);

                    await transaction.CommitAsync();
                    return "Staff Bio Request Request Approved";
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
                    var requestRecord = await _dbContext.Staff.FirstOrDefaultAsync(r => r.Id == RequestDetailPolymorphicId);

                    if (requestRecord is null)
                    {
                        throw new Exception("Record was not found");
                    }
                    requestRecord.updatedAt = DateTime.UtcNow;
                    requestRecord.isApproved = false;
                    requestRecord.isAlterable = true;

                    var authUser = await _authProvider.GetAuthUser();

                    var rejectionNotification = new BiodataRejectNotification
                    {
                        rejectedOn = DateTime.UtcNow,
                        rejectedBy = authUser,
                        author = requestRecord,
                        description = query
                    };

                    // Handling request record status update
                    await _dbContext.StaffRequest.Where(x => x.Id == requestObject.Id)
                        .ExecuteUpdateAsync(r =>
                        r.SetProperty(p => p.updatedAt, DateTime.UtcNow)
                        .SetProperty(p => p.status, StaffRequestStatusTypes.rejected)
                    );
                    await _dbContext.SaveChangesAsync();

                    var notificationObject = new NotificationRecord<BiodataRejectNotification>(rejectionNotification, typeof(Staff).Name, requestRecord.Id);
                    await new Notify<BiodataRejectNotification>(_dbContext).dispatch(notificationObject);
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


    public static class NewStaffBioUpdate
    {
        public class NewStaffBioRequest : IRequest<Shared.Result<Staff>>
        {
            public string title { get; set; } = string.Empty;
            public string GPSAddress { get; set; } = string.Empty;
            public string firstName { get; set; } = string.Empty;
            public string lastName { get; set; } = string.Empty;
            public string? otherNames { get; set; } = string.Empty;
            public string phone { get; set; } = string.Empty;
            public string gender { get; set; } = string.Empty;
            public string SNNITNumber { get; set; } = string.Empty;
            public string email { get; set; } = string.Empty;
            public string disability { get; set; } = string.Empty;
            public string ECOWASCardNumber { get; set; } = string.Empty;
        };

        public class Validator : AbstractValidator<NewStaffBioRequest>
        {
            protected readonly IServiceScopeFactory _serviceScopeFactory;
            public Validator(IServiceScopeFactory scopeServiceFactory)
            {
                _serviceScopeFactory = scopeServiceFactory;

                RuleFor(x => x.email)
                    .NotEmpty()
                    .MustAsync(async (email, cancellationToken) =>
                    {
                        using (var scope = _serviceScopeFactory.CreateScope())
                        {
                            var dbContext = scope.ServiceProvider.GetRequiredService<HRMDBContext>();
                            var authProvider = scope.ServiceProvider.GetRequiredService<Authprovider>();
                            var authStaff = await authProvider.GetAuthStaff();

                            bool exist = await dbContext
                            .Staff
                            .AnyAsync(e => e.email.ToLower() == email.Trim().ToLower() && e.Id != authStaff.Id);
                            return !exist;
                        }

                    })
                    .WithMessage("Email Already Exist")
                    ;
                RuleFor(c => c.firstName).NotEmpty();
                RuleFor(c => c.lastName).NotEmpty();
                RuleFor(c => c.phone).NotEmpty();
                RuleFor(c => c.gender).NotEmpty();
                RuleFor(c => c.SNNITNumber).NotEmpty();
            }
        }

        internal sealed class Handler : IRequestHandler<NewStaffBioRequest, HRM_BACKEND_VSA.Shared.Result<Staff>>
        {
            private readonly HRMDBContext _dbContext;
            private readonly HRMStaffDBContext _staffDbContext;
            private readonly Authprovider _authProvider;
            private readonly IValidator<NewStaffBioRequest> _validator;
            private readonly RequestService _requestService;
            private readonly IMapper _mapper;
            public Handler(IValidator<NewStaffBioRequest> validator, Authprovider authProvider, HRMDBContext dbContext, HRMStaffDBContext staffDbContext, RequestService requestService, IMapper mapper)
            {
                _validator = validator;
                _authProvider = authProvider;
                _dbContext = dbContext;
                _staffDbContext = staffDbContext;
                _requestService = requestService;
                _mapper = mapper;
            }
            public async Task<HRM_BACKEND_VSA.Shared.Result<Staff>> Handle(NewStaffBioRequest request, CancellationToken cancellationToken)
            {
                var validationResult = await _validator.ValidateAsync(request);

                if (validationResult.IsValid is false)
                {
                    return HRM_BACKEND_VSA.Shared.Result.Failure<Staff>(HRM_BACKEND_VSA.Shared.Error.ValidationError(validationResult));
                }

                var authStaff = await _authProvider.GetAuthStaff();
                if (authStaff == null) { return HRM_BACKEND_VSA.Shared.Result.Failure<Staff>(Error.NotFound); }

                if (authStaff.isApproved == false)
                {
                    return Shared.Result.Failure<Staff>(Error.BadRequest("You Have Unprocessed Accomodation Request"));
                }

                using (var dbTransaction = await _dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        //Check if staff has a bio history of this is first 
                        var hasAnyBioChangeHistory = await _dbContext
                            .StaffBioUpdateHistory
                            .AnyAsync(record => record.staffId == authStaff.Id);

                        if (hasAnyBioChangeHistory is false)
                        {
                            var newBioHistoryRecord = _mapper.Map<StaffBioUpdateHistory>(authStaff);
                            newBioHistoryRecord.isApproved = true;
                            _dbContext.StaffBioUpdateHistory.Add(newBioHistoryRecord);
                            await _dbContext.SaveChangesAsync();
                        }

                        var newEntry = await _dbContext.Staff
                             .UpdateOrCreate(_dbContext, authStaff?.Id, new Staff
                             {
                                 updatedAt = DateTime.UtcNow,
                                 title = request.title,
                                 GPSAddress = request.GPSAddress,
                                 firstName = request.firstName,
                                 lastName = request.lastName,
                                 otherNames = request.otherNames,
                                 phone = request.phone,
                                 gender = request.gender,
                                 SNNITNumber = request.SNNITNumber,
                                 email = request.email,
                                 disability = request.disability,
                                 ECOWASCardNumber = request.ECOWASCardNumber,
                                 isApproved = false,
                                 isAlterable = false
                             });

                        var requestService = _requestService.getRequestService(RegisterationRequestTypes.bioData);

                        if (requestService is null)
                        {
                            return HRM_BACKEND_VSA.Shared.Result.Failure<Staff>(Error.BadRequest("Failed To Resolve Required Request Service"));
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
                        return Shared.Result.Failure<Staff>(Error.BadRequest(ex.Message));
                    }

                }
            }
        }
    }
}


public class MapNewBioDataEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/staff-request/biodata",
        [Authorize(Policy = AuthorizationDecisionType.Staff)]
        async (ISender sender, NewStaffBioRequest request) =>
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

        }).WithTags("Staff-Request");
    }
}