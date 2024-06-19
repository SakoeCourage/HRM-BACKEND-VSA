using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace HRM_BACKEND_VSA.Entities
{
    [Index(nameof(specialityName), IsUnique = true)]

    public class Speciality
    {
        [Key]
        public Guid Id { get; set; }
        public Guid categoryId { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime updatedAt { get; set; } = DateTime.UtcNow;
        public string specialityName { get; set; } = String.Empty;
        public virtual Category category { get; set; }
        public ICollection<Staff.Staff> staffs { get; set; }
    }
}
