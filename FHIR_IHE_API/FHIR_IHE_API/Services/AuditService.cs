using FHIR_IHE_API.Data;
using FHIR_IHE_API.Identity;
using FHIR_IHE_API.Models;
using System.Security.Claims;


namespace FHIR_IHE_API.Services
{
    public class AuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuditService> _logger;
        public const string AuditAlreadyLoggedKey = "AuditAlreadyLogged";

        public AuditService(ApplicationDbContext context, ILogger<AuditService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task LogAsync(HttpContext httpContext, string action, string? resourceType = null,
            string? resourceId = null, int? statusCode = null, ApplicationUser? applicationUser = null,
            string? explicitRole = null)
        {
            try
            {
                // ----------------------------------------
                // Get authenticated user information
                // ----------------------------------------

                var user = httpContext.User;

                // --------------------------------------------------
                // User identity
                // --------------------------------------------------

                var userId = applicationUser?.Id ?? user.FindFirstValue(ClaimTypes.NameIdentifier);

                var email = applicationUser?.Email ??
                            user.FindFirstValue(ClaimTypes.Email) ?? user.FindFirstValue(ClaimTypes.Name);

                var role = explicitRole ?? user.FindFirstValue(ClaimTypes.Role);

                // --------------------------------------------------
                // Patient identity
                // --------------------------------------------------

                var patientId = applicationUser?.PatientId;

                if (!patientId.HasValue)
                {
                    var patientIdClaim = user.FindFirstValue(ApplicationClaimTypes.PatientId);

                    if (int.TryParse(patientIdClaim, out var parsedPatientId))
                    {
                        patientId = parsedPatientId;
                    }
                }

                // --------------------------------------------------
                // Provider identity
                // --------------------------------------------------

                var providerId = applicationUser?.ProviderId;

                if (!providerId.HasValue)
                {
                    var providerIdClaim = user.FindFirstValue(ApplicationClaimTypes.ProviderId);

                    if (int.TryParse(providerIdClaim, out var parsedProviderId))
                    {
                        providerId = parsedProviderId;
                    }
                }

                // ----------------------------------------
                // Response status
                // ----------------------------------------

                var finalStatusState = statusCode ?? httpContext.Response.StatusCode;


                // ----------------------------------------
                // Create audit record
                // ----------------------------------------

                var auditing = new AuditLog
                {
                    UserId = userId,
                    UserEmail = email,
                    Role = role,

                    PatientId = patientId,
                    ProviderId = providerId,

                    Action = action,
                    ResourceType = resourceType,
                    ResourceId = resourceId,

                    HttpMethod = httpContext.Request.Method,
                    RequestPath = httpContext.Request.Path,

                    StatusCode = finalStatusState,
                    Success = finalStatusState is >= 200 and < 300,

                    IpAddress = httpContext.Connection.RemoteIpAddress?.ToString(),
                    TimestampUtc = DateTime.UtcNow
                };

                // ----------------------------------------
                // Save
                // ----------------------------------------

                _context.AuditLogs.Add(auditing);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to write audit log!");
            }
        }

        public async Task LogPatientAccessAsync(
            HttpContext httpContext,
            string action,
            int targetPatientId,
            string? resourceId = null,
            int? statusCode = null)
        {
            try
            {
                var user = httpContext.User;

                var userId =
                    user.FindFirstValue(ClaimTypes.NameIdentifier);

                var email =
                    user.FindFirstValue(ClaimTypes.Email)
                    ?? user.FindFirstValue(ClaimTypes.Name);

                var role =
                    user.FindFirstValue(ClaimTypes.Role);

                int? actorPatientId = null;

                var patientIdClaim =
                    user.FindFirstValue(ApplicationClaimTypes.PatientId);

                if (int.TryParse(patientIdClaim, out var parsedPatientId))
                {
                    actorPatientId = parsedPatientId;
                }

                int? providerId = null;

                var providerIdClaim =
                    user.FindFirstValue(ApplicationClaimTypes.ProviderId);

                if (int.TryParse(providerIdClaim, out var parsedProviderId))
                {
                    providerId = parsedProviderId;
                }

                var finalStatusCode =
                    statusCode ?? httpContext.Response.StatusCode;

                var auditing = new AuditLog
                {
                    UserId = userId,
                    UserEmail = email,
                    Role = role,

                    // Actor identity
                    PatientId = actorPatientId,
                    ProviderId = providerId,

                    // Target
                    TargetPatientId = targetPatientId,

                    // Target resource
                    ResourceType = "Patient",
                    ResourceId = resourceId,

                    Action = action,

                    HttpMethod = httpContext.Request.Method,
                    RequestPath = httpContext.Request.Path,

                    StatusCode = finalStatusCode,
                    Success = finalStatusCode >= 200 && finalStatusCode < 300,

                    IpAddress =
                        httpContext.Connection.RemoteIpAddress?.ToString(),

                    TimestampUtc = DateTime.UtcNow
                };

                _context.AuditLogs.Add(auditing);

                await _context.SaveChangesAsync();
                // Mark that the audit has been logged for this request
                // Tell AuditMiddleware that this request
                // has already been audited.
                httpContext.Items[AuditAlreadyLoggedKey] = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to write audit log for Patient {TargetPatientId}", targetPatientId);
            }
        }
    }
}
