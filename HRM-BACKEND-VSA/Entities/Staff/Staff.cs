using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HRM_BACKEND_VSA.Entities.Staff
{
    [Index(nameof(staffIdentificationNumber), IsUnique = true)]
    [Index(nameof(email), IsUnique = true)]
    [Index(nameof(SNNITNumber), IsUnique = true)]
    [Index(nameof(ECOWASCardNumber), IsUnique = true)]
    [Index(nameof(phone), IsUnique = true)]
    public class Staff
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime? updatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? lastSeen { get; set; }
        public string title { get; set; } = string.Empty;
        public string GPSAddress { get; set; } = string.Empty;
        public string staffIdentificationNumber { get; set; }
        public string firstName { get; set; } = string.Empty;
        public string lastName { get; set; } = string.Empty;
        public string? otherNames { get; set; } = string.Empty;
        public Guid? specialityId { get; set; }
        public DateOnly? dateOfBirth { get; set; }
        public string phone { get; set; } = string.Empty;
        public string gender { get; set; } = string.Empty;
        public string SNNITNumber { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string disability { get; set; } = string.Empty;
        public string? passportPicture { get; set; } = string.Empty;
        public string ECOWASCardNumber { get; set; } = string.Empty;
        public Boolean isApproved { get; set; } = false;
        public Boolean isAlterable { get; set; } = false;
        [JsonIgnore]
        public string password { get; set; }
        public virtual User user { get; set; }
        public virtual Unit unit { get; set; }
        public virtual Speciality speciality { get; set; }
        public virtual StaffBankDetail bankDetail { get; set; }
        public virtual StaffFamilyDetail familyDetail { get; set; }
        public virtual StaffProfessionalLincense professionalLincense { get; set; }
        public virtual ICollection<StaffChildrenDetail> staffChildren { get; set; }
        public virtual StaffAccomodationDetail staffAccomodation { get; set; }
        public virtual StaffAppointment currentAppointment { get; set; }
        public virtual ICollection<StaffAppointmentHistory> appointmentHistory { get; set; }


    }
}
