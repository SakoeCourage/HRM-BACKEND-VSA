using HRM_BACKEND_VSA.Entities.HR_Manag;

namespace HRM_BACKEND_VSA.Entities.Notification.Accomodation_Request_Notification
{
    public class AccomdationAcceptNotification
    {
        public DateTime acceptedOn { get; set; } = DateTime.Now;
        public User? acceptedBy { get; set; }
        public Staff.Staff? author { get; set; }
        public StaffRequest request { get; set; }
        public string? description { get; set; } = String.Empty;
    }
}
