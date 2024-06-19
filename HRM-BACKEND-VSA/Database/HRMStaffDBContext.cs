using HRM_BACKEND_VSA.Entities.Applicant;
using Microsoft.EntityFrameworkCore;

namespace HRM_BACKEND_VSA.Database
{
    public class HRMStaffDBContext : DbContext
    {

        public HRMStaffDBContext(DbContextOptions<HRMStaffDBContext> options)
            : base(options)
        {

        }

        public DbSet<Applicant> Applicant { get; set; }
        public DbSet<ApplicantHasOTP> ApplicantHasOTP { get; set; }
        public DbSet<ApplicantEducationalBackground> ApplicantEducationalBackground { get; set; }
        public DbSet<ApplicantBioData> ApplicantBioData { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Applicant>()
            .HasOne(e => e.bioData)
            .WithOne(e => e.Applicant)
            .HasForeignKey<ApplicantBioData>(e => e.applicantId)
            .OnDelete(deleteBehavior: DeleteBehavior.Cascade);

            modelBuilder.Entity<ApplicantBioData>()
                .HasMany(e => e.educationalBackground)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ApplicantBioData>()
                .HasMany(e => e.educationalBackground)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
