using HRM_BACKEND_VSA.Entities;
using HRM_BACKEND_VSA.Entities.Staff;

namespace HRM_BACKEND_VSA.Contracts
{
    public class UserContracts
    {
        public static class UserCredentials
        {
        }

        public class StaffLoginResponsePartial
        {
            public Guid Id { get; set; } 
            public string title { get; set; } = string.Empty;
            public string firstName { get; set; } = string.Empty;
            public string lastName { get; set; } = string.Empty;
            public string? otherNames { get; set; } = string.Empty;
            public string email { get; set; } = string.Empty;
            public string phoneNumber { get; set; } = string.Empty;
            public string staffIdentificationNumber { get; set; } = string.Empty;
            public string? profilePictureUrl { get; set; }
        }

        public class UserLoginResponse
        {
            public Guid Id { get; set; }
            public DateTime createdAt { get; set; } = DateTime.UtcNow;
            public DateTime updatedAt { get; set; } = DateTime.UtcNow;
            public DateTime? emailVerifiedAt { get; set; }
            public DateTime? lastSeen { get; set; }
            public Boolean isAccountActive { get; set; } = true;
            public Boolean hasResetPassword { get; set; } = false;
            public string email { get; set; }
            public StaffLoginResponsePartial staff { get; set; }
            public Role role { get; set; }
            public string accessToken { get; set; }
        }
    }
}