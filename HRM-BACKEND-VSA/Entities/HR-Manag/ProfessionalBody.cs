using HRM_BACKEND_VSA.Entities.Staff;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace HRM_BACKEND_VSA.Entities.HR_Manag
{
    public class ProfessionalBody
    {
        [Key]
        public Guid Id { get; set; }
        public string name { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime? updatedAt { get; set; } = DateTime.UtcNow;
        public virtual Collection<StaffProfessionalLincense> staffProfessionalLincense { get; set; }
    }
}
