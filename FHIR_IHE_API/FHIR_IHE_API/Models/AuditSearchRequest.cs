namespace FHIR_IHE_API.Models
{
    public class AuditSearchRequest
    {
        public int? PatientId { get; set; }

        public int? ProviderId { get; set; }

        public int? TargetPatientId { get; set; }

        public string? Action { get; set; }

        public string? ResourceType { get; set; }

        //FHIR Resource ID
        public string? ResourceId { get; set; }

        public int? StatusCode { get; set; }

        public DateTime? FromUtc { get; set; }

        public DateTime? ToUtc { get; set; }

        // GET /api/audit => default to page 1
        // GET /api/audit?page=2 => page = 2
        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 50;
    }
}
