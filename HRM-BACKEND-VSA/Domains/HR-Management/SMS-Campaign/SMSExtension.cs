using HRM_BACKEND_VSA.Contracts;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Entities.Applicant;
using HRM_BACKEND_VSA.Model.SMS;
using HRM_BACKEND_VSA.Serivices.Mail_Service;
using HRM_BACKEND_VSA.Services.SMS_Service;
using HRM_BACKEND_VSA.Shared;
using HRM_BACKEND_VSA.Utilities;
using Microsoft.AspNetCore.Mvc;
using static HRM_BACKEND_VSA.Contracts.SMSContracts;

namespace HRM_BACKEND_VSA.SMS_Campaign
{
    public class SMSExtension : Controller
    {
        private readonly HRMStaffDBContext _staffDBcontext;
        private readonly HRMDBContext _context;
        private readonly SMSService _smsService;
        private readonly MailService _mailService;

        public SMSExtension(HRMStaffDBContext staffDBcontext, HRMDBContext context, SMSService smsService, MailService mailService)
        {
            _staffDBcontext = staffDBcontext;
            _smsService = smsService;
            _context = context;
            _mailService = mailService;
        }
        public async Task HandleCampaignWithTemplateFile([FromForm] NewFileTemplateSMSDTO requestData)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
               .SelectMany(v => v.Errors)
               .Select(e => e.ErrorMessage)
               .ToList();

                if (errors?.Any() == true)
                {
                    throw new Exception(string.Join(",", errors));
                }
            }

            try
            {


                #region
                SMSTemplate? smsTemplate = null;

                if (requestData?.smsTemplateId is not null)
                {
                    smsTemplate = await _context.SMSTemplate.FindAsync(requestData?.smsTemplateId);
                    if (smsTemplate is null)
                    {
                        throw new Exception("SMS Template Not Found");
                    }

                }

                var smsReceipients = Contactutilities.GetContactsFromFile(requestData.templateFile);
                if (smsReceipients.Count == 0) throw new Exception("Failed to resolve any contact");

                var anyContactNumberError = smsReceipients.Any(i => string.IsNullOrWhiteSpace(i.contact) || i.contact.Length < 9);
                if (anyContactNumberError) throw new Exception("Failed to resolve some contact(s)");

                var contacts = smsReceipients.Select(i => i.contact).ToList();

                bool hasDuplicateContacts = contacts
                    .GroupBy(contact => contact)
                    .Any(g => g.Count() > 1);

                if (hasDuplicateContacts) throw new Exception("Failed to resolve duplicate contacts");
                #endregion

                //var existingApplicantWithSameContacts = await _staffDBcontext.Applicant
                //    .Where(a => contacts.Contains(a.contact))
                //    .Select(a => a.contact)
                //    .ToListAsync();

                //if (existingApplicantWithSameContacts.Count > 0)
                //{
                //    var existingUniqueContact = existingApplicantWithSameContacts.GroupBy(c => c).Select(g => g.Key);
                //    throw new Exception($"Failed to resolve already existing contact(s): {string.Join(",", existingUniqueContact)}");
                //}
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {

                    try
                    {
                        var smsMessage = requestData.message;

                        if (smsTemplate is not null)
                        {
                            smsMessage = smsTemplate.message;
                        }

                        if (smsTemplate?.name == "New Recruitment")
                        {
                            smsMessage = smsTemplate?.message;
                        }

                        var newCampaign = new SMSCampaignHistory
                        {
                            createdAt = DateTime.UtcNow,
                            updatedAt = DateTime.UtcNow,
                            campaignName = requestData.campaingName,
                            smsTemplateId = requestData?.smsTemplateId,
                            message = smsMessage,
                            receipients = smsReceipients.Count()
                        };

                        var newSMSHistory = await _context.SMSCampaignHistory.AddAsync(newCampaign);
                        await _context.SaveChangesAsync();

                        var newCampaignId = newSMSHistory.Entity.Id;

                        List<SMSCampaignReceipient> newReceipientsList = smsReceipients
                            .Select(r => new SMSCampaignReceipient
                            {
                                firstName = r.firstName,
                                lastName = r.lastName,
                                contact = r.contact,
                                message = smsTemplate?.name == "New Recruitment" ? Utilities
                                .Stringutilities.
                                generateNewRecruitMessageTemplate(smsMessage, $"{r.firstName} {r.lastName}")
                                : smsMessage,
                                campaignHistoryId = newCampaignId,
                                email = r.email,
                                status = SMSStatus.successfull
                            }).ToList();


                        List<EmailDTO> emailRecipient = smsReceipients.Select(r => new EmailDTO
                        {
                            ToName = $"{r.firstName} {r.lastName}",
                            ToEmail = r.email,
                            Subject = requestData.campaingName,
                            Body = smsTemplate?.name == "New Recruitment" ? Utilities
                                .Stringutilities.
                                generateNewRecruitMessageTemplate(smsMessage, $"{r.firstName} {r.lastName}")
                                : smsMessage

                        }).ToList();

                        if (smsTemplate?.name == "New Recruitment")
                        {
                            List<Applicant> newApplicantList = smsReceipients.
                                Select(r => new Applicant
                                {
                                    firsName = r.firstName,
                                    lastName = r.lastName,
                                    email = r.email,
                                    contact = r.contact,
                                    createdAt = DateTime.UtcNow,
                                    updatedAt = DateTime.UtcNow
                                }).ToList();
                            await _staffDBcontext.Applicant.AddRangeAsync(newApplicantList);
                        }

                        await _context.SMSCampaignReceipient.AddRangeAsync(newReceipientsList);
                        await _staffDBcontext.SaveChangesAsync();
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                        _smsService.AddRange(newReceipientsList).SendBatchSMS();
                        _mailService.AddRange(emailRecipient).SendBatchEmail();
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }




        public async Task HandleCampaignWithNoTemplateFile(List<SMSCampaignReceipient> newReceipientsList, NewNonFileTemplateSMSDTO requestData)
        {

            try
            {
                var smsMessage = requestData?.message;

                if (newReceipientsList.Count() < 1)
                {
                    throw new Exception("Failed To Resolve SMS Recipients");

                }
                SMSTemplate? smsTemplate = null;

                if (requestData?.smsTemplateId is not null)
                {
                    Console.WriteLine($"Template Message is {requestData.smsTemplateId}");

                    smsTemplate = await _context.SMSTemplate.FindAsync(requestData?.smsTemplateId);
                    if (smsTemplate is null)
                    {
                        throw new Exception("SMS Template Not Found");
                    }
                }


                List<EmailDTO> emailRecipient = newReceipientsList.Select(r => new EmailDTO
                {
                    ToName = $"{r.firstName} {r.lastName}",
                    ToEmail = r.email,
                    Subject = requestData.campaingName,
                    Body = smsMessage

                }).ToList();

                if (smsTemplate is not null)
                {
                    smsMessage = smsTemplate.message;
                    Console.WriteLine($"Template Message is {smsMessage}");
                }

                var newCampaign = new SMSCampaignHistory
                {
                    createdAt = DateTime.UtcNow,
                    updatedAt = DateTime.UtcNow,
                    campaignName = requestData.campaingName,
                    smsTemplateId = requestData?.smsTemplateId,
                    message = smsMessage,
                    receipients = newReceipientsList.Count()
                };

                var newSMSHistory = await _context.SMSCampaignHistory.AddAsync(newCampaign);
                await _context.SaveChangesAsync();

                var newCampaignId = newSMSHistory.Entity.Id;


                await _context.SMSCampaignReceipient.AddRangeAsync(newReceipientsList);
                await _staffDBcontext.SaveChangesAsync();
                await _context.SaveChangesAsync();
                _smsService.AddRange(newReceipientsList).SendBatchSMS();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }

}



