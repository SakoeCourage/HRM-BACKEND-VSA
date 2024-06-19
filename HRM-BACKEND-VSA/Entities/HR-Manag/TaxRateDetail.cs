using System.ComponentModel.DataAnnotations;

namespace HRM_BACKEND_VSA.Entities
{
    public class TaxRateDetail
    {
        [Key]
        public Guid Id { get; set; }
        public Guid taxRateId { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime updatedAt { get; set; } = DateTime.UtcNow;
        public string description { get; set; } = String.Empty;
        public Double taxableIncome { get; set; }
        public Double rate { get; set; }
        public string taxCode { get; set; }
        public virtual TaxRate TaxRate { get; set; }
    }
}
