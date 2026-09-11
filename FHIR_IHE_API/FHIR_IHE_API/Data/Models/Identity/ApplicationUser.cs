using Microsoft.AspNetCore.Identity;

namespace FHIR_IHE_API.Data.Models.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public int? PatientId { get; set; }
    }
}
