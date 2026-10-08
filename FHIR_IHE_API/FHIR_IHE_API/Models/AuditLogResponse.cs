namespace FHIR_IHE_API.Models
{
    public class AuditLogResponse
    {
        public long Id { get; set; }

        // Actor / who performed the action
        public string? UserId { get; set; }

        public string? UserEmail { get; set; }

        public string? Role { get; set; }

        // Actor identity
        public int? PatientId { get; set; }

        public int? ProviderId { get; set; }

        // Target / which Patient was affected
        public int? TargetPatientId { get; set; }

        // Action / resource
        public string Action { get; set; } = string.Empty;

        public string? ResourceType { get; set; }

        public string? ResourceId { get; set; }

        // HTTP information
        public string? HttpMethod { get; set; }

        public string? RequestPath { get; set; }

        // Result
        public int StatusCode { get; set; }

        public bool Success { get; set; }

        // Request information
        public string? IpAddress { get; set; }

        public DateTime TimestampUtc { get; set; }

        public static AuditLogResponse FromEntity(AuditLog auditLog)
        {
            return new AuditLogResponse
            {
                Id = auditLog.Id,

                UserId = auditLog.UserId,
                UserEmail = auditLog.UserEmail,
                Role = auditLog.Role,

                PatientId = auditLog.PatientId,
                ProviderId = auditLog.ProviderId,

                TargetPatientId = auditLog.TargetPatientId,

                Action = auditLog.Action,
                ResourceType = auditLog.ResourceType,
                ResourceId = auditLog.ResourceId,

                HttpMethod = auditLog.HttpMethod,
                RequestPath = auditLog.RequestPath,

                StatusCode = auditLog.StatusCode,
                Success = auditLog.Success,

                IpAddress = auditLog.IpAddress,

                TimestampUtc = auditLog.TimestampUtc
            };

        }

    }
}
