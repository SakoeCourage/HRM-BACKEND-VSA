using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HRM_BACKEND_VSA.Entities.Applicant
{

    public class Applicant
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime updatedAt { get; set; } = DateTime.UtcNow;
        public string firsName { get; set; } = string.Empty;
        public string lastName { get; set; } = string.Empty;
        public string? email { get; set; }
        public string contact { get; set; } = string.Empty;
        public bool? hasSubmittedApplication { get; set; } = false;
        public string? applicationStatus { get; set; } = String.Empty;
        [JsonIgnore]
        public virtual ApplicantHasOTP otp { get; set; }
        public virtual ApplicantBioData bioData { get; set; }

    }
}
