using Carter;
using FluentValidation;
using HRM_BACKEND_VSA.Contracts;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Domains.HR_Management.Staff_Request.StaffRequestHandlers;
using HRM_BACKEND_VSA.Domains.Staffs.Services;
using HRM_BACKEND_VSA.Domains.Staffs.StaffRequestHandlers;
using HRM_BACKEND_VSA.Entities.HR_Manag;
using HRM_BACKEND_VSA.Entities.Notification.Bank_Request_Notification;
using HRM_BACKEND_VSA.Entities.Staff;
using HRM_BACKEND_VSA.Extensions;
using HRM_BACKEND_VSA.Providers;
using HRM_BACKEND_VSA.Serivices.Notification_Service;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Domains.HR_Management.StaffRequestHandlers.NewBankRequest;

namespace HRM_BACKEND_VSA.Domains.HR_Management.StaffRequestHandlers
{
    public class BankRequest : IStaffRequest
    {
        private readonly HRMDBContext _dbContext;
        private readonly Authprovider _authProvider;
        public BankRequest(HRMDBContext dbContext, Authprovider authProvider)
        {
            _dbContext = dbContext;
            _authProvider = authProvider;
        }
        public async Task<object> GetStaffRequestData(Guid RequestDetailPolymorphicId)
        {
            var requestData = await _dbContext.StaffBankDetail.FirstOrDefaultAsync(x => x.Id == RequestDetailPolymorphicId);
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
                requestType = RegisterationRequestTypes.bankUpdate,
                status = StaffRequestStatusTypes.pending,
                requestFromStaffId = authStaff?.Id,
                RequestDetailPolymorphicId = RequestDetailPolymorphicId,
                requestAssignedStaffId = assignedUser.staffId
            };


            _dbContext.StaffRequest.Add(newRegisterationData);

            var requestNotification = new NewBankRequestNotifcation
            {
                author = authStaff,
                requestDate = DateTime.UtcNow,
                description = "",
                request = newRegisterationData
            };

            try
            {
                await _dbContext.SaveChangesAsync();
                await new Notify<NewBankRequestNotifcation>(_dbContext)
                 .dispatch(
                 new NotificationRecord<NewBankRequestNotifcation>(requestNotification, typeof(Entities.User).Name, assignedUser.Id)
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
                    var bankRecord = await _dbContext.StaffBankDetail.IgnoreAutoIncludes().Include(sa => sa.staff).FirstOrDefaultAsync(r => r.Id == RequestDetailPolymorphicId);
                    if (bankRecord is null)
                    {
                        throw new Exception("Record was not found");
                    }
                    bankRecord.isApproved = true;
                    bankRecord.isAlterable = true;

                    _dbContext.StaffBankUpdateHistory.Add(new StaffBankUpdateHistory
                    {
                        createdAt = DateTime.UtcNow,
                        updatedAt = DateTime.UtcNow,
                        staffId = bankRecord.staffId,
                        isApproved = true,
                        bankId = bankRecord.bankId,
                        accountType = bankRecord.accountType,
                        accountNumber = bankRecord.accountNumber,
                        branch = bankRecord.branch
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
                    var acceptionNotifcation = new BankAcceptNotification
                    {
                        acceptedOn = DateTime.UtcNow,
                        acceptedBy = authUser,
                        author = bankRecord.staff,
                        description = String.Empty
                    };
                    var notificationObject = new NotificationRecord<BankAcceptNotification>(acceptionNotifcation, typeof(Staff).Name, bankRecord.staff.Id);

                    await new Notify<BankAcceptNotification>(_dbContext).dispatch(notificationObject);
                    await transaction.CommitAsync();
                    return "Staff Bank Request Approved";
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task OnRequestRejected(Guid id, string? query, StaffRequest requestObject)
        {
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var requestRecord = await _dbContext.StaffBankDetail.Include(ar => ar.staff).FirstOrDefaultAsync(r => r.Id == id);

                    if (requestRecord is null)
                    {
                        throw new Exception("Record was not found");
                    }
                    requestRecord.updatedAt = DateTime.UtcNow;
                    requestRecord.isApproved = false;
                    requestRecord.isAlterable = true;

                    var authUser = await _authProvider.GetAuthUser();

                    var rejectionNotification = new BankRejectNotification
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

                    var notificationObject = new NotificationRecord<BankRejectNotification>(rejectionNotification, typeof(Staff).Name, requestRecord.staff.Id);
                    await new Notify<BankRejectNotification>(_dbContext).dispatch(notificationObject);
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

    public static class NewBankRequest
    {
        public class NewBankRequestData : IRequest<Shared.Result<StaffBankDetail>>
        {
            public Guid bankId { get; set; }
            public string accountType { get; set; }
            public string branch { get; set; }
            public string accountNumber { get; set; }
        }

        public class Validator : AbstractValidator<NewBankRequestData>
        {
            public Validator()
            {
                RuleFor(c => c.bankId).NotEmpty();
                RuleFor(c => c.accountType).NotEmpty();
                RuleFor(c => c.branch).NotEmpty();
                RuleFor(c => c.accountNumber).NotEmpty();
            }
        }
        internal sealed class Handler : IRequestHandler<NewBankRequestData, Shared.Result<StaffBankDetail>>
        {
            private readonly HRMDBContext _dbContext;
            private readonly HRMStaffDBContext _staffDbContext;
            private readonly Authprovider _authProvider;
            private readonly IValidator<NewBankRequestData> _validator;
            private readonly RequestService _requestService;
            public Handler(IValidator<NewBankRequestData> validator, Authprovider authProvider, HRMDBContext dbContext, HRMStaffDBContext staffDbContext, RequestService requestService)
            {
                _validator = validator;
                _authProvider = authProvider;
                _dbContext = dbContext;
                _staffDbContext = staffDbContext;
                _requestService = requestService;
            }
            public async Task<Shared.Result<StaffBankDetail>> Handle(NewBankRequestData request, CancellationToken cancellationToken)
            {

                var validationResult = _validator.Validate(request);

                if (validationResult.IsValid is false)
                {
                    return Shared.Result.Failure<StaffBankDetail>(Error.ValidationError(validationResult));
                }

                var authStaff = await _authProvider.GetAuthStaff();
                if (authStaff == null) { return Shared.Result.Failure<StaffBankDetail>(Error.NotFound); };

                var currentBankDbRecord = await _dbContext.StaffAccomodationDetail.FirstOrDefaultAsync(sa => sa.staffId == authStaff.Id);

                if (currentBankDbRecord is not null)
                {
                    if (currentBankDbRecord.isApproved is false)
                    {
                        return Shared.Result.Failure<StaffBankDetail>(Error.BadRequest("You Have An Unprocessed Bank Request"));
                    }
                }

                using (var dbTransaction = await _dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var newEntry = await _dbContext.StaffBankDetail
                             .UpdateOrCreate(_dbContext, currentBankDbRecord?.Id ?? null, new StaffBankDetail
                             {
                                 updatedAt = DateTime.UtcNow,
                                 staffId = authStaff.Id,
                                 bankId = request.bankId,
                                 accountType = request.accountType,
                                 accountNumber = request.accountNumber,
                                 branch = request.branch,
                                 isApproved = false,
                                 isAlterable = false
                             });

                        var requestService = _requestService.getRequestService(RegisterationRequestTypes.bankUpdate);

                        if (requestService is null)
                        {
                            return Shared.Result.Failure<StaffBankDetail>(Error.BadRequest("Failed To Resolve Required Request Service"));
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
                        return Shared.Result.Failure<StaffBankDetail>(Error.BadRequest(ex.Message));

                    }

                }
            }
        }

    }
}

public class MapNewBankRequestEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/staff-request/bank-update",
        [Authorize(Policy = AuthorizationDecisionType.Staff)]
        async (ISender sender, NewBankRequestData request) =>
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


