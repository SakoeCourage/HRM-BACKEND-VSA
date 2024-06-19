namespace HRM_BACKEND_VSA.Entities.HR_Manag
{
    public class RoleHasPermissions
    {
        public Guid roleId { get; set; }
        public Guid permissionId { get; set; }

        public virtual Role role { get; set; }
        public virtual Permission permission { get; set; }
    }
}
