namespace HRM_BACKEND_VSA.Entities.HR_Manag
{
    public class UserHasRole
    {
        public Guid userId { get; set; }
        public Guid roleId { get; set; }
        public virtual User user { get; set; }
        public virtual Role role { get; set; }
    }
}
