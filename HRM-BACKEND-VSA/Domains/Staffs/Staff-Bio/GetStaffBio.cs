using HRM_BACKEND_VSA.Contracts;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRM_BACKEND_VSA.Domains.Staffs.Staff_Bio;

public class GetStaffBio
{
    public class GetStaffBioRequest:IRequest<Shared.Result<StaffContracts.StaffProfileResponse>>
    {   
        public Guid id { get; set; }
    }

    internal sealed class handler(HRMDBContext dbContext):IRequestHandler<GetStaffBioRequest, Shared.Result<StaffContracts.StaffProfileResponse>>
    {
        public async Task<Result<StaffContracts.StaffProfileResponse>> Handle(GetStaffBioRequest request, CancellationToken cancellationToken)
        {
            var staff = await dbContext.Staff.Where(st => st.Id == request.id)
                .Include(entry => entry.staffAccomodation)
                .Include(entry => entry.professionalLincense)
                .Include(entry => entry.speciality)
                .Include(entry => entry.unit)
                .Include(entry => entry.staffChildren)
                .FirstOrDefaultAsync();
                ;
                return null;
        }
    }
}