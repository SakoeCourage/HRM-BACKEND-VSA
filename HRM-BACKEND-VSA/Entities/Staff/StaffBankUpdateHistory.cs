using System.ComponentModel.DataAnnotations;

namespace HRM_BACKEND_VSA.Entities.Staff
{
    public class StaffBankUpdateHistory
    {
        [Key]
        public Guid Id { get; set; }
        public Guid staffId { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime? updatedAt { get; set; } = DateTime.UtcNow;
        public Guid bankId { get; set; }
        public string accountType { get; set; }
        public string branch { get; set; }
        public string accountNumber { get; set; }
        public Boolean isApproved { get; set; }
        public virtual Staff staff { get; set; }
    }
}
