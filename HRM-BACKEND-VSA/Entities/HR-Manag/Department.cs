using System.ComponentModel.DataAnnotations;

namespace HRM_BACKEND_VSA.Entities
{
    public class Department
    {
        [Key]
        public Guid Id { get; set; }
        public Guid directorateId { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime updatedAt { get; set; } = DateTime.UtcNow;
        public string departmentName { get; set; }
        public Guid? headOfDepartmentId { get; set; }
        public Guid? depHeadOfDepartmentId { get; set; }
        public virtual Staff.Staff headOfDepartment { get; set; }
        public virtual Staff.Staff depHeadOfDepartment { get; set; }
        public virtual Directorate directorate { get; set; }
        public virtual ICollection<Unit> units { set; get; }
        public virtual ICollection<User> users { get; set; }


    }
}
