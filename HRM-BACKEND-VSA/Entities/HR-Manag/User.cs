using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HRM_BACKEND_VSA.Entities
{
    [Index(nameof(email), IsUnique = true)]
    public class User
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime updatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? emailVerifiedAt { get; set; }
        public DateTime? lastSeen { get; set; }
        public Boolean isAccountActive { get; set; } = true;
        public Boolean hasResetPassword { get; set; } = false;
        public Guid staffId { get; set; }
        public Guid roleId { get; set; }
        public Guid unitId { get; set; }
        public Guid departmentId { get; set; }
        public string email { get; set; }
        [JsonIgnore]
        public string password { get; set; }
        public virtual Staff.Staff staff { get; set; }
        public virtual Role role { get; set; }
        public virtual Unit unit { get; set; }
        public virtual Department department { get; set; }
    }
}
