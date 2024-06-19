using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace HRM_BACKEND_VSA.Entities
{
    [Index(nameof(code), IsUnique = true)]
    public class Allowance
    {

        [Key]
        public Guid Id { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime updatedAt { get; set; } = DateTime.UtcNow;
        public string code { get; set; } = String.Empty;
        public Double allowance { get; set; }

    }
}
