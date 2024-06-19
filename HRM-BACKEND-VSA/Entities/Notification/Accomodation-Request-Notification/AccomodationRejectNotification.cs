﻿using HRM_BACKEND_VSA.Entities.HR_Manag;

namespace HRM_BACKEND_VSA.Entities.Notification.Accomodation_Request_Notification
{
    public class AccomodationRejectNotification
    {
        public DateTime rejectedOn { get; set; } = DateTime.Now;
        public User? rejectedBy { get; set; }
        public Staff.Staff? author { get; set; }
        public StaffRequest request { get; set; }
        public string? description { get; set; } = String.Empty;
    }
}
