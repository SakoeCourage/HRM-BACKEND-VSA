using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace HRM_BACKEND_VSA.Entities
{
    [Index(nameof(year), IsUnique = true)]

    public class TaxRate
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime updatedAt { get; set; } = DateTime.UtcNow;
        public DateOnly year { get; set; }
        public virtual ICollection<TaxRateDetail> taxRateDetails { get; set; }
    }
}
