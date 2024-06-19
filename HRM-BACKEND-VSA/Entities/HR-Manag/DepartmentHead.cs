using System.ComponentModel.DataAnnotations;

namespace HRM_BACKEND_VSA.Entities.HR_Manag
{
    public class DepartmentHead
    {
        [Key]
        public Guid Id { get; set; }
        public Guid departmentId { get; set; }
        public string position { get; set; } = String.Empty;
        public Guid staffId { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime updatedAt { get; set; } = DateTime.UtcNow;
        public virtual Department department { get; set; }
    }
}
