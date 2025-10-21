using Microsoft.EntityFrameworkCore;
using FHIR_IHE_API.Models;

namespace FHIR_IHE_API.Data
{
    
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<Patient> Patients { get; set; }

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
