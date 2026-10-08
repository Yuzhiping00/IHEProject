using System.ComponentModel.DataAnnotations;

namespace FHIR_IHE_API.Models
{
    public class Provider
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string GivenName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FamilyName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ProviderNumber { get; set; }
    }
}