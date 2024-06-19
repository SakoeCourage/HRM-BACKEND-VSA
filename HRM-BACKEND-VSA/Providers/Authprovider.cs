using AutoMapper;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Entities;
using HRM_BACKEND_VSA.Entities.Applicant;
using HRM_BACKEND_VSA.Entities.Staff;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HRM_BACKEND_VSA.Providers
{
    public static class AuthorizationDecisionType
    {
        public const string Applicant = "Applicant";
        public const string Staff = "Staff";
        public const string HRMUser = "HRMUser";
    }
    public class Authprovider
    {
        private readonly HRMDBContext _dbContext;
        private readonly HttpContext _httpContext;
        private readonly IMapper _mapper;
        private readonly HRMStaffDBContext _staffDBContext;

        public Authprovider(IServiceScopeFactory serviceScopeFactory)
        {
            var scope = serviceScopeFactory.CreateScope();
            _dbContext = scope.ServiceProvider.GetRequiredService<HRMDBContext>();
            _staffDBContext = scope.ServiceProvider.GetRequiredService<HRMStaffDBContext>();
            _httpContext = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext;
            _mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        }

        public async Task<Applicant?> GetAuthApplicant()
        {

            ClaimsIdentity identity = _httpContext?.User.Identity as ClaimsIdentity;

            if (identity == null) return null;

            Claim ApplicantIdClaim = identity.FindFirst(ClaimTypes.NameIdentifier);
            Claim authorizationDecisionClaim = identity.FindFirst(ClaimTypes.AuthorizationDecision);

            var applicantId = ApplicantIdClaim?.Value;
            var applicantAuthorizationValue = authorizationDecisionClaim?.Value;

            if (applicantId is not null && applicantAuthorizationValue is not null)
            {
                if (applicantAuthorizationValue != AuthorizationDecisionType.Applicant.ToString()) return null;

                var applicant = await _staffDBContext.Applicant.FirstOrDefaultAsync(a => a.Id == Guid.Parse(applicantId));
                return applicant;
            }
            return null;
        }

        public async Task<Staff?> GetAuthStaff()
        {
            ClaimsIdentity identity = _httpContext?.User.Identity as ClaimsIdentity;

            if (identity == null) return null;

            Claim StaffIdClaim = identity.FindFirst(ClaimTypes.NameIdentifier);
            Claim authorizationDecisionClaim = identity.FindFirst(ClaimTypes.AuthorizationDecision);

            var staffId = StaffIdClaim?.Value;
            var applicantAuthorizationValue = authorizationDecisionClaim?.Value;

            if (staffId is not null && applicantAuthorizationValue is not null)
            {
                if (applicantAuthorizationValue != AuthorizationDecisionType.Staff) return null;

                var staff = await _dbContext.Staff.FirstOrDefaultAsync(a => a.Id == Guid.Parse(staffId));
                return staff;
            }
            return null;
        }


        public async Task<User?> GetAuthUser()
        {
            ClaimsIdentity identity = _httpContext?.User.Identity as ClaimsIdentity;

            if (identity == null) return null;

            Claim UserIdClaim = identity.FindFirst(ClaimTypes.NameIdentifier);
            Claim authorizationDecisionClaim = identity.FindFirst(ClaimTypes.AuthorizationDecision);

            var userId = UserIdClaim?.Value;
            var applicantAuthorizationValue = authorizationDecisionClaim?.Value;

            if (userId is not null && applicantAuthorizationValue is not null)
            {
                if (applicantAuthorizationValue != AuthorizationDecisionType.HRMUser) return null;

                var user = await _dbContext
                    .User
                    .IgnoreAutoIncludes()
                    .Include(u => u.staff)
                    .FirstOrDefaultAsync(a => a.Id == Guid.Parse(userId));
                return user;
            }
            return null;
        }
    }
}
