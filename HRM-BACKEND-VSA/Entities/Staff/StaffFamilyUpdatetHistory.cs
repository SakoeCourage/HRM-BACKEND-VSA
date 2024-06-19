using System.ComponentModel.DataAnnotations;

namespace HRM_BACKEND_VSA.Entities.Staff
{
    public class StaffFamilyUpdatetHistory
    {
        [Key]
        public Guid Id { get; set; }
        public Guid staffId { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime? updatedAt { get; set; } = DateTime.UtcNow;
        public string fathersName { get; set; } = String.Empty;
        public string mothersName { get; set; } = String.Empty;
        public string spouseName { get; set; } = String.Empty;
        public string spousePhoneNumber { get; set; } = String.Empty;
        public string nextOfKIN { get; set; } = String.Empty;
        public string nextOfKINPhoneNumber { get; set; } = String.Empty;
        public string emergencyPerson { get; set; } = String.Empty;
        public string emergencyPersonPhoneNumber { get; set; } = String.Empty;
        public virtual Staff staff { get; set; }
        public Boolean isApproved { get; set; }
    }
}
