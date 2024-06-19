namespace HRM_BACKEND_VSA.Entities.Applicant
{
    public class ApplicantEducationalBackground
    {
        public Guid Id { get; set; }
        public Guid applicantBioDataId { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime updatedAt { get; set; } = DateTime.UtcNow;
        public DateOnly yearCompleted { get; set; }
        public string institutionName { get; set; } = string.Empty;
        public string certificate { get; set; } = string.Empty;

    }
}
