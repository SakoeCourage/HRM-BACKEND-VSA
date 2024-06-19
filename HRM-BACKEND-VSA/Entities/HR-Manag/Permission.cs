using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HRM_BACKEND_VSA.Entities
{
    [Index(nameof(name), IsUnique = true)]
    public class Permission
    {
        [Key]
        public Guid Id { get; set; }
        public string name { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime? updatedAt { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public virtual ICollection<Role> roles { get; set; }

    }
}
