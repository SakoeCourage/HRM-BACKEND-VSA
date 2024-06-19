using HRM_BACKEND_VSA.Entities.HR_Manag;

namespace HRM_BACKEND_VSA.Entities.Notification.Accomodation_Request_Notification
{
    public class NewAccomodationRequestNotification
    {
        public DateTime requestDate { get; set; } = DateTime.Now;
        public Staff.Staff author { get; set; }
        public StaffRequest request { get; set; }
        public string description { get; set; } = String.Empty;
    }
}
