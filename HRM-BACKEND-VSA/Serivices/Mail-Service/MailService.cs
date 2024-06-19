using Hangfire;
using HRM_BACKEND_VSA.Contracts;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
namespace HRM_BACKEND_VSA.Serivices.Mail_Service
{
    public class MailService
    {
        private List<EmailDTO> emailBatchList;
        private readonly IConfiguration _configuration;
        private readonly IConfigurationSection _siteSetting;
        private readonly ILogger<MailService> _logger;
        public MailService(IConfiguration _configuration, ILogger<MailService> logger)
        {
            _configuration = _configuration;
            _siteSetting = _configuration.GetSection("siteSettings");
            _logger = logger;
            emailBatchList = new List<EmailDTO>();

        }

        public MailService AddToBatchSMS(EmailDTO recipientData)
        {
            emailBatchList.Add(recipientData);
            return this;
        }

        public MailService AddRange(List<EmailDTO> list)
        {
            emailBatchList.AddRange(list);
            return this;
        }

        public async void SendBatchEmail()
        {
            await SendBatchEmailJob();
        }

        [AutomaticRetry(Attempts = 3)]
        public async Task SendBatchEmailJob()
        {
            _logger.LogInformation("Batch Email Job Has Started...");
            foreach (var message in emailBatchList)
            {
                BackgroundJob.Enqueue(() => SendEmailAsync(message));
            }
            _logger.LogInformation("Batch Eamil Job completed.");
        }



        public async Task SendEmailAsync(EmailDTO Reqmessage)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_siteSetting["mailFromName"], _siteSetting["mailFormAddress"]));
            message.To.Add(new MailboxAddress(Reqmessage.ToName, Reqmessage.ToEmail));
            message.Subject = Reqmessage.Subject;

            message.Body = new TextPart(TextFormat.Html)
            {
                Text = MailTemplateWrapper.wrappMailBody(Reqmessage.Body)
            }; ;

            using (var client = new SmtpClient())
            {
                client.Connect(_siteSetting["mailHost"], int.Parse(_siteSetting["mailPort"]), false);

                client.Authenticate(_siteSetting["mailUserName"], _siteSetting["mailPassword"]);

                client.Send(message);
                client.Disconnect(true);
            }
        }

        [AutomaticRetry(Attempts = 3)]
        public void SendMail(EmailDTO message)
        {
            BackgroundJob.Enqueue(() => this.SendEmailAsync(message));
        }
    }
}


