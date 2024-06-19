using HRM_BACKEND_VSA.Entities;
using HRM_BACKEND_VSA.Entities.Applicant;
using HRM_BACKEND_VSA.Entities.HR_Manag;
using HRM_BACKEND_VSA.Entities.Notification;
using HRM_BACKEND_VSA.Entities.Staff;
using HRM_BACKEND_VSA.Model.SMS;
using Microsoft.EntityFrameworkCore;

namespace HRM_BACKEND_VSA.Database
{
    public class HRMDBContext : DbContext
    {
        public HRMDBContext(DbContextOptions<HRMDBContext> options)
            : base(options)
        {


        }
        public DbSet<SMSTemplate> SMSTemplate { get; set; }
        public DbSet<Bank> Bank { get; set; }
        public DbSet<SMSCampaignHistory> SMSCampaignHistory { get; set; }
        public DbSet<SMSCampaignReceipient> SMSCampaignReceipient { get; set; }
        public DbSet<ApplicantBioData> ApplicantBioData { get; set; }
        public DbSet<Grade> Grade { get; set; }
        public DbSet<GradeStep> GradeStep { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<TaxRate> TaxRate { get; set; }
        public DbSet<TaxRateDetail> TaxRateDetail { get; set; }
        public DbSet<Allowance> Allowance { get; set; }
        public DbSet<Speciality> Speciality { get; set; }
        public DbSet<Role> Role { get; set; }
        public DbSet<Permission> Permission { get; set; }
        public DbSet<RoleHasPermissions> RoleHasPermissions { get; set; }
        public DbSet<Directorate> Directorate { get; set; }
        public DbSet<Department> Department { get; set; }
        public DbSet<Unit> Unit { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<UserHasRole> UserHasRole { get; set; }
        public DbSet<Staff> Staff { get; set; }
        public DbSet<StaffBankDetail> StaffBankDetail { get; set; }
        public DbSet<StaffFamilyDetail> StaffFamilyDetail { get; set; }
        public DbSet<StaffRequest> StaffRequest { get; set; }
        public DbSet<ProfessionalBody> ProfessionalBody { get; set; }
        public DbSet<StaffChildrenDetail> StaffChildrenDetail { get; set; }
        public DbSet<StaffProfessionalLincense> StaffProfessionalLincense { get; set; }
        public DbSet<StaffAccomodationDetail> StaffAccomodationDetail { get; set; }
        public DbSet<StaffBioUpdateHistory> StaffBioUpdateHistory { get; set; }
        public DbSet<StaffFamilyUpdatetHistory> StaffFamilyUpdatetHistory { get; set; }
        public DbSet<StaffProfessionalLincenseUpdateHistory> StaffProfessionalLincenseUpdateHistory { get; set; }
        public DbSet<StaffChildrenUpdateHistory> StaffChildrenUpdateHistory { get; set; }
        public DbSet<StaffAccomodationUpdateHistory> StaffAccomodationUpdateHistory { get; set; }
        public DbSet<StaffBankUpdateHistory> StaffBankUpdateHistory { get; set; }
        public DbSet<UserHasOTP> UserHasOTP { get; set; }
        public DbSet<StaffAppointment> StaffAppointment { get; set; }
        public DbSet<StaffAppointmentHistory> StaffAppointmentHistory { get; set; }
        public DbSet<StaffPosting> StaffPosting { get; set; }
        public DbSet<StaffPostingHistory> StaffPostingHistory { get; set; }
        public DbSet<Notification> Notification { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<StaffPosting>()
                 .HasOne(u => u.unit)
                 .WithOne()
                 .HasForeignKey<StaffPosting>(sp => sp.unitId)
                 .OnDelete(DeleteBehavior.ClientCascade);

            modelBuilder.Entity<StaffPosting>()
                 .HasOne(u => u.department)
                 .WithOne()
                 .HasForeignKey<StaffPosting>(sp => sp.departmentId)
                 .OnDelete(DeleteBehavior.ClientCascade);

            modelBuilder.Entity<StaffPosting>()
                 .HasOne(u => u.directorate)
                 .WithOne()
                 .HasForeignKey<StaffPosting>(sp => sp.directorateId)
                 .OnDelete(DeleteBehavior.ClientCascade);

            modelBuilder.Entity<StaffPosting>()
                 .HasOne(sp => sp.Staff)
                 .WithOne()
                 .HasForeignKey<StaffPosting>(sp => sp.staffId)
                 .OnDelete(DeleteBehavior.ClientCascade);

            modelBuilder.Entity<StaffPostingHistory>()
                .HasOne(sp => sp.Staff)
                .WithMany()
                .HasForeignKey(sp => sp.staffId)
                .OnDelete(DeleteBehavior.ClientCascade);

            modelBuilder.Entity<StaffAppointment>()
                .HasOne(s => s.staff)
                .WithOne(s => s.currentAppointment)
                .HasForeignKey<StaffAppointment>(s => s.staffId)
                .OnDelete(DeleteBehavior.ClientCascade);

            modelBuilder.Entity<StaffAppointment>()
                .HasOne(s => s.grade)
                .WithMany(s => s.appointments)
                .HasForeignKey(s => s.gradeId)
                .OnDelete(DeleteBehavior.NoAction);


            modelBuilder.Entity<StaffAppointment>()
                .HasOne(s => s.speciality)
               .WithMany()
               .HasForeignKey(sa => sa.staffSpecialityId)
               .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<StaffAppointmentHistory>()
                .HasOne(s => s.speciality)
                .WithMany()
                .HasForeignKey(sa => sa.staffSpecialityId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<StaffAppointmentHistory>()
                .HasOne(s => s.staff)
                .WithMany(s => s.appointmentHistory)
                .HasForeignKey(s => s.staffId)
                .OnDelete(DeleteBehavior.ClientCascade);

            modelBuilder.Entity<StaffAppointmentHistory>()
                .HasOne(s => s.grade)
                .WithMany()
                .HasForeignKey(s => s.gradeId)
                .OnDelete(DeleteBehavior.NoAction);


            modelBuilder.Entity<StaffBioUpdateHistory>()
                .HasOne(s => s.staff)
                .WithMany()
                .HasForeignKey(s => s.staffId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StaffAccomodationUpdateHistory>()
                .HasOne(s => s.staff)
                .WithMany()
                .HasForeignKey(s => s.staffId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StaffBankUpdateHistory>()
                .HasOne(s => s.staff)
                .WithMany()
                .HasForeignKey(s => s.staffId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StaffChildrenUpdateHistory>()
                .HasOne(s => s.staff)
                .WithMany()
                .HasForeignKey(s => s.staffId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StaffBioUpdateHistory>()
                .HasOne(s => s.staff)
                .WithMany()
                .HasForeignKey(s => s.staffId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StaffProfessionalLincense>()
                .HasOne(pl => pl.ProfessionalBody)
                .WithMany(pb => pb.staffProfessionalLincense)
                .HasForeignKey(pl => pl.professionalBodyId)
                .OnDelete(DeleteBehavior.NoAction);


            modelBuilder.Entity<StaffChildrenDetail>()
                .HasOne(s => s.staff)
                .WithMany(sc => sc.staffChildren)
                .HasForeignKey(sc => sc.staffId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Staff>()
                .HasOne(e => e.staffAccomodation)
                .WithOne(pl => pl.staff)
                .HasForeignKey<StaffAccomodationDetail>(e => e.staffId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Staff>()
                .HasOne(e => e.professionalLincense)
                .WithOne(pl => pl.staff)
                .HasForeignKey<StaffProfessionalLincense>(e => e.staffId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<SMSCampaignHistory>()
               .HasOne(e => e.smsTemplate)
               .WithMany(e => e.smsHistory)
               .HasForeignKey(e => e.smsTemplateId);

            modelBuilder.Entity<SMSCampaignReceipient>()
                .HasOne(e => e.campaignHistory)
                .WithMany(e => e.smsReceipients)
                .HasForeignKey(e => e.campaignHistoryId);

            modelBuilder.Entity<SMSCampaignReceipient>(entity =>
            {
                entity.Property(e => e.status)
                .HasComment("Status => Pending or Successful or Failed");
            });

            modelBuilder.Entity<Category>()
                .HasMany(e => e.grades)
                .WithOne(e => e.category)
                .HasForeignKey(e => e.categoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Category>()
                .HasMany(e => e.specialities)
                .WithOne(e => e.category)
                .HasForeignKey(e => e.categoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Grade>()
                .HasMany(e => e.steps)
                .WithOne(e => e.grade)
                .HasForeignKey(e => e.gradeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaxRate>()
                .HasMany(e => e.taxRateDetails)
                .WithOne(e => e.TaxRate)
                .HasForeignKey(e => e.taxRateId)
                .OnDelete(DeleteBehavior.ClientCascade);

            modelBuilder.Entity<Role>()
                   .HasMany(r => r.permissions)
                   .WithMany(p => p.roles)
                   .UsingEntity<RoleHasPermissions>(
                       j => j
                           .HasOne(rp => rp.permission)
                           .WithMany()
                           .HasForeignKey(rp => rp.permissionId)
                           .OnDelete(DeleteBehavior.Cascade)
                           ,
                       j => j
                           .HasOne(rp => rp.role)
                           .WithMany()
                           .HasForeignKey(rp => rp.roleId)
                           .OnDelete(DeleteBehavior.Cascade)
                           ,
                       j =>
                       {
                           j.HasKey(rp => new { rp.roleId, rp.permissionId });
                           j.ToTable("RoleHasPermissions");
                       });

            modelBuilder.Entity<Role>()
                .HasMany(r => r.users)
                .WithOne(u => u.role)
                .HasForeignKey(r => r.roleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Speciality>()
                .HasMany(s => s.staffs)
                .WithOne(s => s.speciality)
                .HasForeignKey(s => s.specialityId);


            modelBuilder.Entity<Directorate>()
                .HasOne(d => d.director)
                .WithOne()
                .HasForeignKey<Directorate>(e => e.directorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Directorate>()
                .HasOne(d => d.depDirector)
                .WithOne()
                .HasForeignKey<Directorate>(e => e.depDirectoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Directorate>()
               .HasMany(d => d.units)
               .WithOne(u => u.directorate)
               .HasForeignKey(e => e.directorateId)
               .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Department>()
                .HasMany(s => s.units)
                .WithOne(s => s.department)
                .HasForeignKey(s => s.departmentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Department>()
                .HasOne(d => d.depHeadOfDepartment)
                .WithOne()
                .HasForeignKey<Department>(e => e.depHeadOfDepartmentId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Department>()
              .HasOne(d => d.headOfDepartment)
              .WithOne()
              .HasForeignKey<Department>(e => e.headOfDepartmentId)
              .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Department>()
                .HasOne(d => d.directorate)
                .WithMany(d => d.departments)
                .HasForeignKey(d => d.directorateId)
                .OnDelete(deleteBehavior: DeleteBehavior.NoAction);

            modelBuilder.Entity<Staff>()
                .HasOne(s => s.bankDetail)
                .WithOne(b => b.staff)
                .HasForeignKey<StaffBankDetail>(b => b.staffId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Staff>()
                .HasOne(s => s.familyDetail)
                .WithOne(b => b.staff)
                .HasForeignKey<StaffFamilyDetail>(b => b.staffId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StaffRequest>()
                .HasOne(s => s.requestFromStaff)
                .WithMany()
                .HasForeignKey(s => s.requestAssignedStaffId);

            modelBuilder.Entity<StaffRequest>()
                .HasOne(s => s.requestAssignedStaff)
                .WithMany()
                .HasForeignKey(s => s.requestAssignedStaffId);

            modelBuilder.Entity<StaffBankDetail>()
               .HasOne(s => s.bank)
               .WithMany(u => u.staffbankDetails)
               .HasForeignKey(s => s.bankId)
               .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<User>()
                .HasOne(s => s.role)
                .WithMany(s => s.users)
                .HasForeignKey(u => u.roleId)
                .HasPrincipalKey(u => u.Id);

            modelBuilder.Entity<User>()
                .HasOne(s => s.staff)
                .WithOne(u => u.user)
                .HasForeignKey<User>(u => u.staffId)
                .OnDelete(DeleteBehavior.ClientCascade);

            modelBuilder.Entity<User>()
                .HasOne(s => s.department)
                .WithMany(s => s.users)
                .HasForeignKey(u => u.departmentId);

            modelBuilder.Entity<User>()
                .HasOne(s => s.unit)
                .WithMany(s => s.users)
                .HasForeignKey(u => u.unitId);

            modelBuilder.Entity<UserHasRole>()
            .HasKey(sr => new { sr.userId, sr.roleId })
            ;

            modelBuilder.Entity<Unit>()
                .HasOne(u => u.department)
                .WithMany(d => d.units)
                .HasForeignKey(d => d.departmentId)
                .OnDelete(deleteBehavior: DeleteBehavior.NoAction);

            modelBuilder.Entity<Unit>()
                .HasOne(u => u.unitHead)
                .WithOne(s => s.unit)
                .HasForeignKey<Unit>(u => u.unitHeadId)
                .OnDelete(deleteBehavior: DeleteBehavior.NoAction);
        }
    }
}
