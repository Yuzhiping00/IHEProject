using System.ComponentModel.DataAnnotations;

namespace FHIR_IHE_API.Models
{
    public class AuditLog
    {
        [Key]
        public long Id { get; set; }

        // The authenticated user's identifier.
        // This can be null for anonymous requests
        // such as failed login attempts.
        public string? UserId { get; set; }

        // Email or username associated with the request.
        public string? UserEmail { get; set; }

        // Provider / Patient / etc.
        public string? Role { get; set; }

        public int? PatientId { get; set; }

        public int? ProviderId { get; set; }

        // What happened.
        // Examples:
        // GET
        // POST
        // LOGIN_SUCCESS
        // LOGIN_FAILED
        public string Action { get; set; } = string.Empty;

        // The type of resource being accessed.
        // Example: Patient
        public string? ResourceType { get; set; }

        // The identifier of the resource.
        // Example: FHIR Patient ID.
        public string? ResourceId { get; set; }

        // HTTP method.
        // GET / POST / PUT / DELETE
        public string? HttpMethod { get; set; }

        // API endpoint.
        // Example:
        // /api/patient/123
        public string? RequestPath { get; set; }

        // HTTP response status.
        // 200, 201, 401, 403, 404, etc.
        public int StatusCode { get; set; }

        // Whether the request was successful.
        public bool Success { get; set; }

        // Client IP address.
        public string? IpAddress { get; set; }

        // When the event occurred.
        public DateTime TimestampUtc { get; set; }
    }
}
