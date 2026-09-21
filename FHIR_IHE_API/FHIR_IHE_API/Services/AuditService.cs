using FHIR_IHE_API.Data;
using FHIR_IHE_API.Models;
using System.Security.Claims;


namespace FHIR_IHE_API.Services
{
    public class AuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuditService> _logger;

        public AuditService(ApplicationDbContext context, ILogger<AuditService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task LogAsync(HttpContext httpContext, string action, string? resourceType = null,
            string? resourceId = null, int? statusCode = null)
        {
            try
            {
                // ----------------------------------------
                // Get authenticated user information
                // ----------------------------------------

                var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var email = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;

                // Your current JWT uses ClaimTypes.Name, 
                // so use it as a fallback

                if (string.IsNullOrWhiteSpace(email))
                {
                    email = httpContext.User.FindFirst(ClaimTypes.Name)?.Value;
                }

                var role = httpContext.User.FindFirst(ClaimTypes.Role)?.Value;

                // ----------------------------------------
                // Determine status code
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
                    Action = action,
                    ResourceType = resourceType,
                    ResourceId = resourceId,
                    HttpMethod = httpContext.Request.Method,
                    RequestPath = httpContext.Request.Path.Value,
                    StatusCode = finalStatusState,
                    Success = finalStatusState >= 200 && finalStatusState <= 400,
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
    }
}
