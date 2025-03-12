using System.ComponentModel.DataAnnotations;

namespace FHIR_IHE_API.Models
{
    
    public class LoginRequest
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
}
