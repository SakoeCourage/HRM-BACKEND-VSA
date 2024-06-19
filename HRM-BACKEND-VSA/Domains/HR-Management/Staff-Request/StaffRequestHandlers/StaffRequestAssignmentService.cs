using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Entities.Staff;
using Microsoft.EntityFrameworkCore;

namespace HRM_BACKEND_VSA.Domains.HR_Management.Staff_Request.StaffRequestHandlers
{
    public class StaffRequestAssignmentService
    {
        private readonly HRMDBContext _dbContext;
        public StaffRequestAssignmentService(HRMDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Guid?> ElegibleStaffForAssignment(string abiility)
        {
            var assignedStaff = await _dbContext.Staff.FirstOrDefaultAsync();

            if (assignedStaff != null)
            {
                return assignedStaff?.Id;
            }
            return null;
        }

        public async Task<Staff?> ElegibleStaffEntityForAssignment(string abiility)
        {
            var assignedStaff = await _dbContext.Staff.FirstOrDefaultAsync();

            if (assignedStaff != null)
            {
                return assignedStaff;
            }
            return null;
        }

        public async Task<Entities.User?> EligibleUserEntityForAssingment(string abiility)
        {
            var assignendedUser = await _dbContext.User.Include(u => u.staff).FirstOrDefaultAsync();

            if (assignendedUser != null)
            {
                return assignendedUser;
            }
            return null;
        }
    }
}
