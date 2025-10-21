using Microsoft.EntityFrameworkCore;
using FHIR_IHE_API.Models;

namespace FHIR_IHE_API.Data
{
    
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Patient> Patients { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        { 
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.Property(p => p.BirthDate)
                    .HasColumnType("date");
            });
        }
    }
}
