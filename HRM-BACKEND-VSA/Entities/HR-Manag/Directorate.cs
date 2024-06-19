using System.ComponentModel.DataAnnotations;

namespace HRM_BACKEND_VSA.Entities
{
    public class Directorate
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime updatedAt { get; set; } = DateTime.UtcNow;
        public string directorateName { get; set; }
        public Guid? directorId { get; set; }
        public Guid? depDirectoryId { get; set; }
        public virtual Staff.Staff director { get; set; }
        public virtual Staff.Staff depDirector { get; set; }
        public virtual ICollection<Department> departments { get; set; }
        public virtual ICollection<Unit> units { get; set; }
    }
}
