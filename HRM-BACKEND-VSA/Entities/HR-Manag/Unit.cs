using System.ComponentModel.DataAnnotations;

namespace HRM_BACKEND_VSA.Entities
{
    public class Unit
    {
        [Key]
        public Guid Id { get; set; }
        public Guid departmentId { get; set; }
        public Guid? unitHeadId { get; set; }
        public Guid? directorateId { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime updatedAt { get; set; } = DateTime.UtcNow;
        public string unitName { get; set; }
        public virtual Department department { get; set; }
        public virtual Directorate directorate { get; set; }
        public virtual Staff.Staff unitHead { get; set; }
        public virtual ICollection<User> users { get; set; }

    }
}
