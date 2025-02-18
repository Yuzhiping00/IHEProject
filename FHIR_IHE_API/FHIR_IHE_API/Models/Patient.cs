using System.ComponentModel.DataAnnotations;

namespace FHIR_IHE_API.Models
{
    
    public class Patient
    {
        [Key] 
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string? FamilyName { get; set; }

        [Required]
        [MaxLength(100)]
        public string? GivenName { get; set; }

        [Required]
        public string? Gender { get; set; } 

        [Required]
        public DateOnly BirthDate { get; set; }

        public string ResourceType => "Patient";
    }
}
