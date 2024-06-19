using System.ComponentModel.DataAnnotations;

namespace HRM_BACKEND_VSA.Entities.Notification
{
    public class Notification
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime? updatedAt { get; set; } = DateTime.UtcNow;
        public string type { get; set; } = String.Empty;
        public string data { get; set; } = String.Empty;
        public string notifiableType { get; set; } = String.Empty;
        public Guid notifiableId { get; set; } = Guid.Empty;
        public DateTime? readAt { get; set; } = null;
    }
}
