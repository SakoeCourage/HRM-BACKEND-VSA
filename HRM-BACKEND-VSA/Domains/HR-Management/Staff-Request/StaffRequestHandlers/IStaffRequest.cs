using HRM_BACKEND_VSA.Entities.HR_Manag;

namespace HRM_BACKEND_VSA.Domains.Staffs.StaffRequestHandlers
{
    public interface IStaffRequest
    {
        Task<Guid> NewStaffRequest(Guid requestDetailPolymorphicId);
        Task<object> GetStaffRequestData(Guid id);
        Task<string> OnRequestAccepted(Guid requestDetailPolymorphicId, StaffRequest requestObject);
        Task OnRequestRejected(Guid id, string? query, StaffRequest requestObject);

    }
}
