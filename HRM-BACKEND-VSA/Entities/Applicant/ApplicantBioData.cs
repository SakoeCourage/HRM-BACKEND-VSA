using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HRM_BACKEND_VSA.Entities.Applicant
{
    [Index(nameof(phoneOne), IsUnique = true)]
    public class ApplicantBioData
    {
        [Key]
        public Guid Id { get; set; }
        public Guid applicantId { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime updatedAt { get; set; } = DateTime.UtcNow;
        public string title { get; set; } = string.Empty;
        public string surName { get; set; } = string.Empty;
        public string firstName { get; set; } = string.Empty;
        public string? otherNames { get; set; } = string.Empty;
        public string gender { get; set; } = string.Empty;
        public string citizenship { get; set; } = string.Empty;
        public DateOnly? dateOfBirth { get; set; } = null;
        public string? SNNITNumber { get; set; } = string.Empty;
        public string phoneOne { get; set; } = string.Empty;
        public string? phoneTwo { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string? GPSAddress { get; set; } = string.Empty;
        public string? disability { get; set; } = string.Empty;
        public string ECOWASCardNumber { get; set; } = string.Empty;
        public string? passportNumber { get; set; } = string.Empty;
        public string? passportPicture { get; set; }
        public string? birthCertificate { get; set; }
        public string? highestQualification { get; set; } = string.Empty;
        public string? highestQualificationCertificate { get; set; }
        public string? nssNumber { get; set; }
        public DateOnly yearOfService { get; set; }
        public string? placeOfService { get; set; }
        public string? nssCertificate { get; set; } = string.Empty;
        public string? controllerStaffNumber { get; set; } = string.Empty;
        public virtual ICollection<ApplicantEducationalBackground> educationalBackground { get; set; }
        [JsonIgnore]
        public virtual Applicant Applicant { get; set; }

    }
}
