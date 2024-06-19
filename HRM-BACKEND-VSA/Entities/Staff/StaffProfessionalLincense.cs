using HRM_BACKEND_VSA.Entities.HR_Manag;
using System.ComponentModel.DataAnnotations;

namespace HRM_BACKEND_VSA.Entities.Staff
{
    public class StaffProfessionalLincense
    {
        [Key]
        public Guid Id { get; set; }
        public Guid professionalBodyId { get; set; }
        public Guid staffId { get; set; }
        public string pin { get; set; }
        public DateOnly issuedDate { get; set; }
        public DateOnly expiryDate { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime? updatedAt { get; set; } = DateTime.UtcNow;
        public virtual ProfessionalBody ProfessionalBody { get; set; }
        public virtual Staff staff { get; set; }
        public Boolean isApproved { get; set; }
        public Boolean isAlterable { get; set; } = false;
    }
}
