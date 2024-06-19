using FluentValidation;
using HRM_BACKEND_VSA.Contracts;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Domains.HR_Management.Staff_Request.StaffRequestHandlers;
using HRM_BACKEND_VSA.Domains.Staffs.StaffRequestHandlers;
using HRM_BACKEND_VSA.Entities.HR_Manag;
using HRM_BACKEND_VSA.Entities.Notification.New_Registration_Notification;
using HRM_BACKEND_VSA.Extensions;
using HRM_BACKEND_VSA.Providers;
using HRM_BACKEND_VSA.Serivices.Notification_Service;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq.Dynamic.Core;

namespace HRM_BACKEND_VSA.Domains.HR_Management.StaffRequestHandlers
{
    public class NewStaffRegisterationHandler : IStaffRequest
    {
        private readonly HRMDBContext _dbContext;
        private readonly HRMStaffDBContext _staffDbContext;
        private readonly Authprovider _authProvider;
        public NewStaffRegisterationHandler(HRMDBContext dbContext, HRMStaffDBContext staffDbContext, Authprovider authProvider)
        {
            _dbContext = dbContext;
            _staffDbContext = staffDbContext;
            _authProvider = authProvider;
        }
        public async Task<object> GetStaffRequestData(Guid RequestDetailPolymorphicId)
        {
            var applicantBio = await _staffDbContext
                .Applicant
                .Include(e => e.bioData)
                .ThenInclude(e => e.educationalBackground)
                .FirstOrDefaultAsync(x => x.Id == RequestDetailPolymorphicId);

            return applicantBio;
        }

        public async Task<Guid> NewStaffRequest(Guid RequestDetailPolymorphicId)
        {
            var assignedUser = await new StaffRequestAssignmentService(_dbContext)
            .EligibleUserEntityForAssingment(RegisterationRequestTypes.newRegisteration);

            if (assignedUser is null)
            {
                throw new Exception("Failed To Assigned Request To Staff");
            }

            var existingRequest = await _dbContext
                .StaffRequest
                .FirstOrDefaultAsync(x => x.RequestDetailPolymorphicId == RequestDetailPolymorphicId);

            //Update Applicant Data 
            var affetectApplicantRows = await _staffDbContext.Applicant.Where(x => x.Id == RequestDetailPolymorphicId)
                .ExecuteUpdateAsync(setters => setters
                .SetProperty(c => c.updatedAt, DateTime.UtcNow)
                .SetProperty(c => c.applicationStatus, StaffRequestStatusTypes.pending)
                );

            if (affetectApplicantRows == 0)
            {
                throw new Exception("Applicant Not Found");
            }

            var authApplicant = await _authProvider.GetAuthApplicant();

            var newRegisterationData = new StaffRequest
            {
                updatedAt = DateTime.UtcNow,
                requestType = RegisterationRequestTypes.newRegisteration,
                status = StaffRequestStatusTypes.pending,
                requestFromStaffId = null,
                RequestDetailPolymorphicId = RequestDetailPolymorphicId,
                requestAssignedStaffId = assignedUser.staffId
            };

            var newEntry = await _dbContext.StaffRequest.UpdateOrCreate(_dbContext, existingRequest?.Id, newRegisterationData);

            var requestNotification = new NewRegistrationNotification
            {
                author = null,
                applicant = authApplicant,
                requestDate = DateTime.UtcNow,
                description = "",
                request = newRegisterationData
            };

            try
            {
                await _dbContext.SaveChangesAsync();
                await new Notify<NewRegistrationNotification>(_dbContext)
                .dispatch(new NotificationRecord<NewRegistrationNotification>(requestNotification, typeof(Entities.User).Name, assignedUser.Id));
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
                    var dbRecord = await _staffDbContext.Applicant.Include(a => a.bioData).FirstOrDefaultAsync(r => r.Id == RequestDetailPolymorphicId);
                    if (dbRecord is null)
                    {
                        throw new Exception("Record was not found");
                    }
                    dbRecord.applicationStatus = StaffRequestStatusTypes.approved;
                    dbRecord.updatedAt = DateTime.UtcNow;

                    // Handling request record status update
                    await _dbContext.StaffRequest.Where(x => x.Id == requestObject.Id)
                        .ExecuteUpdateAsync(r =>
                        r.SetProperty(p => p.updatedAt, DateTime.UtcNow)
                        .SetProperty(p => p.status, StaffRequestStatusTypes.approved)
                    );

                    await _staffDbContext.SaveChangesAsync();
                    await _dbContext.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return "Staff New Application Approved";
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
                    var requestRecord = await _staffDbContext.Applicant.Include(a => a.bioData).FirstOrDefaultAsync(r => r.Id == RequestDetailPolymorphicId);

                    if (requestRecord is null)
                    {
                        throw new Exception("Record was not found");
                    }
                    requestRecord.updatedAt = DateTime.UtcNow;
                    requestRecord.applicationStatus = StaffRequestStatusTypes.rejected;


                    // Handling request record status update
                    await _dbContext.StaffRequest.Where(x => x.Id == requestObject.Id)
                        .ExecuteUpdateAsync(r =>
                        r.SetProperty(p => p.updatedAt, DateTime.UtcNow)
                        .SetProperty(p => p.status, StaffRequestStatusTypes.rejected)
                    );

                    await _staffDbContext.SaveChangesAsync();
                    await _dbContext.SaveChangesAsync();
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



}



