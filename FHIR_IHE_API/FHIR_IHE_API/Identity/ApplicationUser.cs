using FHIR_IHE_API.Models;
using Microsoft.AspNetCore.Identity;

namespace FHIR_IHE_API.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public int? PatientId { get; set; }

        // navigation property
        public Patient? Patient { get; set; }

        public int? ProviderId { get; set; }

        //navigation property
        public Provider? Provider { get; set; }
    }
}
