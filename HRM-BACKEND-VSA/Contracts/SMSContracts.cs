namespace HRM_BACKEND_VSA.Contracts
{
    public class SMSContracts
    {
        public class NewFileTemplateSMSDTO
        {

            public string campaingName { get; set; }
            public Guid? smsTemplateId { get; set; }
            public IFormFile templateFile { get; set; }
            public string? message { get; set; }
            public string? frequency { get; set; }
        }

        public class NewNonFileTemplateSMSDTO
        {
            public string campaingName { get; set; }
            public Guid? smsTemplateId { get; set; }
            public string? message { get; set; }
            public string? frequency { get; set; }
        }
    }
}
