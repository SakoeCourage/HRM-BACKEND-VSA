using HRM_BACKEND_VSA.Shared;
using System.ComponentModel.DataAnnotations;

namespace HRM_BACKEND_VSA.Model.SMS
{
    public class SMSCampaignReceipient
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public Guid campaignHistoryId { get; set; }
        [Required]
        public string message { get; set; }
        [Required]
        public string contact { get; set; }
        public string email { get; set; }
        public string? firstName { get; set; }
        public string? lastName { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; } = DateTime.UtcNow;
        public string? status { get; set; } = SMSStatus.pending;
        public virtual SMSCampaignHistory campaignHistory { get; set; }

    }
}
