using FHIR_IHE_API.Identity;
using FHIR_IHE_API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FHIR_IHE_API.Data
{

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Patient> Patients { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        public DbSet<Provider> Providers { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // why need base.OnModelCreating(modelBuilder) ? because Identity itself has to create many tables: 
            // AspNetUsers
            // AspNetRoles
            // AspNetUserRoles
            // AspNetUserClaims
            // AspNetUserLogins
            // AspNetUserTokens
            // AspNetRoleClaims

            // without this line of code, the Identity tables will not be created and you will get an error when trying to use Identity features.
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Patient>(entity =>
            {
                entity.Property(p => p.BirthDate)
                    .HasColumnType("date");
            });

            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Patient)
                .WithMany()
                .HasForeignKey(u => u.PatientId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Provider)
                .WithMany()
                .HasForeignKey(u => u.ProviderId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(a => a.Id);

                entity.Property(a => a.UserId)
                    .HasMaxLength(450);

                entity.Property(a => a.UserEmail)
                    .HasMaxLength(320);

                entity.Property(a => a.Role)
                    .HasMaxLength(100);

                entity.Property(a => a.Action)
                    .HasMaxLength(100);

                entity.Property(a => a.ResourceType)
                    .HasMaxLength(100);

                entity.Property(a => a.ResourceId)
                    .HasMaxLength(200);

                entity.Property(a => a.HttpMethod)
                    .HasMaxLength(20);

                entity.Property(a => a.RequestPath)
                    .HasMaxLength(2000);

                entity.Property(a => a.IpAddress)
                    .HasMaxLength(100);

                entity.Property(a => a.TimestampUtc)
                    .IsRequired();
            });
        }
    }
}
