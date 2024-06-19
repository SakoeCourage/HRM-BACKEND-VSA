using HRM_BACKEND_VSA.Entities.Staff;

namespace HRM_BACKEND_VSA.Contracts
{
    public class UserContracts
    {

        public static class UserCredentials
        {


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
            public Guid staffId { get; set; }
            public Guid roleId { get; set; }
            public Guid unitId { get; set; }
            public Guid departmentId { get; set; }
            public string email { get; set; }
            public virtual Staff staff { get; set; }
            public string accessToken { get; set; }
        }
    }
}
