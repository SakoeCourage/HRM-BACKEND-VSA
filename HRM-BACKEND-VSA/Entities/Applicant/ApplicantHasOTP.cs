using System.ComponentModel.DataAnnotations;

namespace HRM_BACKEND_VSA.Entities.Applicant
{
    public class ApplicantHasOTP
    {
        [Key]
        public Guid Id { get; set; }
        public string contact { get; set; } = string.Empty;
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime updatedAt { get; set; } = DateTime.UtcNow;
        public string otp { get; set; }
        public Guid applicantID { get; set; }
        public virtual Applicant applicant { get; set; }
    }
}
