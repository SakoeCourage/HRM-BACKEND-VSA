namespace HRM_BACKEND_VSA.Contracts
{
    public static class RegisterationRequestTypes
    {
        public static readonly string newRegisteration = "new-registeration";
        public static readonly string bioData = "biodata";
        public static readonly string bankUpdate = "bank-update";
        public static readonly string familyDetails = "family-details";
        public static readonly string childrenDetails = "children-details";
        public static readonly string professionalLicense = "professional-licence";
        public static readonly string accomodation = "accommodation";
    }

    public static class StaffTypesRequestList
    {
        public static readonly string managementStaff = "management";
        public static readonly string clinicalStaff = "clinical";
        public static readonly string administrativeStaff = "administrative";
        public static readonly string technicalSupportStaff = "technicalSupport";
    }

    public static class StaffTypesInitials
    {
        private static readonly Dictionary<string, string> requestTypeDictionary = new Dictionary<string, string>
        {
            {StaffTypesRequestList.managementStaff,"MS"},
            {StaffTypesRequestList.clinicalStaff,"CS"},
            {StaffTypesRequestList.administrativeStaff,"AS"},
            {StaffTypesRequestList.technicalSupportStaff,"TS"}
        };

        public static string? getGetInitialsFromStaffRequestType(string requestType)
        {
            string? initial = null;
            requestTypeDictionary.TryGetValue(requestType, out initial);
            return initial;
        }
    }


    public static class PaymentSourceRequestList
    {
        public static readonly string controllerPaymentSource = "CAGD";
        public static readonly string internalFunded = "IGF";
    }

    public static class PaymentSourceResponseInitials
    {
        private static readonly Dictionary<string, string> paymentSourceDictionary = new Dictionary<string, string> {

            {PaymentSourceRequestList.controllerPaymentSource,"C" },
            { PaymentSourceRequestList.internalFunded, "I"}
        };

        public static string? getGetInitialsFromStaffRequestType(string requestType)
        {
            string? initial = null;
            paymentSourceDictionary.TryGetValue(requestType, out initial);
            return initial;
        }
    }

    public static class StaffRequestStatusTypes
    {
        public static readonly string pending = "PENDING";
        public static readonly string appointed = "APPOINTED";
        public static readonly string approved = "APPROVED";
        public static readonly string rejected = "REJECTED";
        public static readonly string posted = "POSTED";
    }
    public class NewStaffRequestData
    {
        public Guid? requestFromStaffId { get; set; }
        public Guid requestAssignedStaffId { get; set; }
        public Guid RequestDetailPolymorphicId { get; set; }
        public string requestType { get; set; } = string.Empty;
        public string status { get; set; } = string.Empty;
    }
}
