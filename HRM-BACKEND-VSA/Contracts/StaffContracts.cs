namespace HRM_BACKEND_VSA.Contracts
{
    public class StaffContracts
    {

        public class StaffLoginResponse
        {
            public Guid Id { get; set; }
            public string staffIdentificationNumber { get; set; }
            public string firstName { get; set; } = string.Empty;
            public string lastName { get; set; } = string.Empty;
            public string? otherNames { get; set; } = string.Empty;
            public Guid? specialityId { get; set; }
            public DateOnly dateOfBirth { get; set; }
            public string phone { get; set; } = string.Empty;
            public string gender { get; set; } = string.Empty;
            public string SNNITNumber { get; set; } = string.Empty;
            public string email { get; set; } = string.Empty;
            public string disability { get; set; } = string.Empty;
            public string? passportPicture { get; set; } = string.Empty;
            public string accessToken { get; set; }
        }
    }
}
