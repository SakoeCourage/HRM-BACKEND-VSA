using HRM_BACKEND_VSA.Entities.HR_Manag;

namespace HRM_BACKEND_VSA.Domains.Staffs.StaffRequestHandlers
{
    public interface IStaffRequest
    {
        Task<Guid> NewStaffRequest(Guid RequestDetailPolymorphicId);
        Task<object> GetStaffRequestData(Guid Id);
        Task<string> OnRequestAccepted(Guid RequestDetailPolymorphicId, StaffRequest requestObject);
        Task OnRequestRejected(Guid id, string? query, StaffRequest requestObject);

    }
}
