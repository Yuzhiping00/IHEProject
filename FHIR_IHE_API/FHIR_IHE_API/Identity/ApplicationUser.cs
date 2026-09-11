using Microsoft.AspNetCore.Identity;

namespace FHIR_IHE_API.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public int? PatientId { get; set; }
    }
}
