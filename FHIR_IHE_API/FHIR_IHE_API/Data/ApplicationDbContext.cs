using FHIR_IHE_API.Data.Models.Identity;
using FHIR_IHE_API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FHIR_IHE_API.Data
{

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Patient> Patients { get; set; }

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
                .HasOne<Patient>()
                .WithMany()
                .HasForeignKey(u => u.PatientId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
