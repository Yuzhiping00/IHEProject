using System.ComponentModel.DataAnnotations;

namespace FHIR_IHE_API.Models
{
    
    public class Provider
    {
       
        [Key] 
        public int Id { get; set; }

        [Required]
        public string? FirstName { get; set; }

        [Required]
        public string? LastName { get; set; }

        [Required]
        public string? Email { get; set; }

        
        [Required]
        public string? Address { get; set; }

        [Required]
        public string? Phone { get; set; }

        public ICollection<Patient>? Patients { get; set; }

    }
}



