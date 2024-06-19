using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Model.SMS;
using HRM_BACKEND_VSA.Serivices.Mail_Service;
using HRM_BACKEND_VSA.Services.SMS_Service;
using HRM_BACKEND_VSA.Shared;
using HRM_BACKEND_VSA.SMS_Campaign;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Contracts.SMSContracts;

namespace HRM_BACKEND_VSA.Domains.HR_Management.SMS_Campaign
{
    public class SMSRequestHandlers
    {
        private readonly HRMDBContext _dbContext;
        private readonly HRMStaffDBContext _staffDBContext;
        private readonly SMSService _smsService;
        private readonly MailService _mailService;
        public SMSRequestHandlers(HRMDBContext dbContext,
            HRMStaffDBContext staffDBContext,
            SMSService smsService,
            MailService mailService
            )
        {
            _dbContext = dbContext;
            _staffDBContext = staffDBContext;
            _smsService = smsService;
            _mailService = mailService;
        }


        public async Task handleOnStaffSMSRequest(Guid[] staffIds, NewNonFileTemplateSMSDTO request)
        {
            if (staffIds.Count() > 0)
            {
                var staffs = await _dbContext
                    .Staff
                    .Where(s => staffIds.Contains(s.Id))
                .ToListAsync();

                var templateRequest = new NewNonFileTemplateSMSDTO
                {
                    smsTemplateId = request.smsTemplateId,
                    campaingName = request.campaingName,
                    message = request.message,
                    frequency = request.frequency

                };
                var newCampaign = new SMSCampaignHistory
                {
                    createdAt = DateTime.UtcNow,
                    updatedAt = DateTime.UtcNow,
                    campaignName = request.campaingName,
                    smsTemplateId = request?.smsTemplateId,
                    message = request?.message,
                    receipients = staffs.Count()
                };

                var newSMSHistory = await _dbContext.SMSCampaignHistory.AddAsync(newCampaign);
                await _dbContext.SaveChangesAsync();

                List<SMSCampaignReceipient> smsRecipients = staffs.Select(st =>
                new SMSCampaignReceipient
                {
                    firstName = st.firstName,
                    lastName = st.lastName,
                    contact = st.phone,
                    message = request?.message,
                    campaignHistoryId = newCampaign.Id,
                    status = SMSStatus.successfull,
                    createdAt = DateTime.UtcNow,
                    updatedAt = DateTime.UtcNow
                }
                ).ToList();
                await new SMSExtension(_staffDBContext, _dbContext, _smsService, _mailService).HandleCampaignWithNoTemplateFile(smsRecipients, templateRequest);
            }
            else
            {
                throw new Exception("Failed To Resolve Staff Recipients");
            }
        }

        public async Task handleOnUnitSMSRequest(string unitId, NewNonFileTemplateSMSDTO request)
        {

            var unit = await _dbContext.Unit.Include(u => u.users).ThenInclude(s => s.staff).FirstOrDefaultAsync(u => u.Id == Guid.Parse(unitId));
            ;
            if (unit is null) throw new Exception("Requested Unit Not Found");

            var templateRequest = new NewNonFileTemplateSMSDTO
            {
                smsTemplateId = request.smsTemplateId,
                campaingName = request.campaingName,
                message = request.message,
                frequency = request.frequency

            };
            var newCampaign = new SMSCampaignHistory
            {
                createdAt = DateTime.UtcNow,
                updatedAt = DateTime.UtcNow,
                campaignName = request.campaingName,
                smsTemplateId = request?.smsTemplateId,
                message = request?.message,
                receipients = unit.users.Count()
            };

            var newSMSHistory = await _dbContext.SMSCampaignHistory.AddAsync(newCampaign);
            await _dbContext.SaveChangesAsync();

            var staffList = unit.users.Select(user =>
            {
                var staffData = user.staff;
                return new SMSCampaignReceipient
                {
                    firstName = staffData.firstName,
                    lastName = staffData.lastName,
                    message = request.message,
                    campaignHistoryId = newCampaign.Id,
                    status = SMSStatus.successfull,
                    contact = staffData.phone,
                    createdAt = DateTime.UtcNow,
                    updatedAt = DateTime.UtcNow
                };
            }).ToList();

            await new SMSExtension(_staffDBContext, _dbContext, _smsService, _mailService).HandleCampaignWithNoTemplateFile(staffList, templateRequest);
        }

        public async Task handleOnDepartmentSMSRequest(string departmentId, NewNonFileTemplateSMSDTO request)
        {

            var department = await _dbContext.Department.Include(u => u.users).ThenInclude(s => s.staff).FirstOrDefaultAsync(u => u.Id == Guid.Parse(departmentId));
            ;
            if (department is null) throw new Exception("Requested Department Not Found");

            var templateRequest = new NewNonFileTemplateSMSDTO
            {
                smsTemplateId = request.smsTemplateId,
                campaingName = request.campaingName,
                message = request.message,
                frequency = request.frequency

            };
            var newCampaign = new SMSCampaignHistory
            {
                createdAt = DateTime.UtcNow,
                updatedAt = DateTime.UtcNow,
                campaignName = request.campaingName,
                smsTemplateId = request?.smsTemplateId,
                message = request?.message,
                receipients = department.users.Count()
            };

            var newSMSHistory = await _dbContext.SMSCampaignHistory.AddAsync(newCampaign);
            await _dbContext.SaveChangesAsync();

            var staffList = department.users.Select(user =>
            {
                var staffData = user.staff;
                return new SMSCampaignReceipient
                {
                    firstName = staffData.firstName,
                    lastName = staffData.lastName,
                    message = request.message,
                    campaignHistoryId = newCampaign.Id,
                    status = SMSStatus.successfull,
                    contact = staffData.phone,
                    createdAt = DateTime.UtcNow,
                    updatedAt = DateTime.UtcNow
                };
            }).ToList();

            await new SMSExtension(_staffDBContext, _dbContext, _smsService, _mailService).HandleCampaignWithNoTemplateFile(staffList, templateRequest);
        }


    }
}
