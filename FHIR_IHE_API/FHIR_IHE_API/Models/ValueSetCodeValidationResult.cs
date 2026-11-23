namespace FHIR_IHE_API.Models
{
    public class ValueSetCodeValidationResult
    {
        public bool IsValid { get; set; }

        public bool ValueSetFound { get; set; }

        public bool SystemFound { get; set; }

        public string System { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public string? ProvidedDisplay { get; set; }

        public string? ExpectedDisplay { get; set; }

        public string? Message { get; set; }
    }
}