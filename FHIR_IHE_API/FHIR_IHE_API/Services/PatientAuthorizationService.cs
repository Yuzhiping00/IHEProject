using FHIR_IHE_API.Identity;
using System.Security.Claims;

namespace FHIR_IHE_API.Services
{
    public class PatientAuthorizationService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public PatientAuthorizationService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Gets the Patient ID associated with the currently authenticated user.
        /// Returns null if the current user is not a patient or has no valid PatientId claim.
        /// </summary>
        ///
        public int? GetCurrentPatientId()
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user == null || user.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            var patientIdClaim = user.FindFirstValue(ApplicationClaimTypes.PatientId);

            if (!int.TryParse(patientIdClaim, out var patientId))
            {
                return null;
            }

            return patientId;
        }

        /// <summary>
        /// Determines whether the currently authenticated user is a provider.
        /// </summary>
        public bool IsProvider()
        {
            var user = _httpContextAccessor.HttpContext?.User;

            return user?.IsInRole(ApplicationRoles.ProviderRole) == true;
        }

        /// <summary>
        /// Determines whether the currently authenticated user is a patient.
        /// </summary>
        public bool IsPatient()
        {
            var user = _httpContextAccessor.HttpContext?.User;

            return user?.IsInRole(ApplicationRoles.PatientRole) == true;
        }

        /// <summary>
        /// Determines whether the current user can access the specified patient.
        ///
        /// Provider:
        ///     Can access any patient.
        ///
        /// Patient:
        ///     Can access only their own patient record.
        /// </summary>
        public bool CanAccessPatient(int patientId)
        {
            if (IsProvider())
            {
                return true;
            }

            var currentPatientId = GetCurrentPatientId();

            if (!currentPatientId.HasValue)
            {
                return false;
            }

            return currentPatientId.Value == patientId;
        }

    }
}
