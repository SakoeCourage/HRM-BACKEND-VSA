using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HRM_BACKEND_VSA.Entities
{

    [Index(nameof(categoryName), IsUnique = true)]
    public class Category
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime updatedAt { get; set; } = DateTime.UtcNow;
        public string categoryName { get; set; } = String.Empty;
        [JsonIgnore]
        public virtual ICollection<Grade> grades { get; set; }
        public virtual ICollection<Speciality> specialities { get; set; }
    }
}
