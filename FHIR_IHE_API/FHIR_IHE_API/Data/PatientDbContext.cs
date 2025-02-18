using Microsoft.EntityFrameworkCore;
using FHIR_IHE_API.Models;

namespace FHIR_IHE_API.Data
{
    
    public class PatientDbContext : DbContext
    {
        public DbSet<Patient> Patients { get; set; }

        public PatientDbContext(DbContextOptions<PatientDbContext> options) : base(options)
        { 
        }
    }
}
