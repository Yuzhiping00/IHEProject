namespace FHIR_IHE_API.Models
{
    public class ValueSetDefinition
    {
        public string Id { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Version { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Status { get; set; } = "active";

        public List<ValueSetIncludeDefinition> Includes { get; set; } = new();
    }

    public class ValueSetIncludeDefinition
    {
        public string System { get; set; } = string.Empty;

        public string? Version { get; set; }

        public List<ValueSetConceptDefinition> Concepts { get; set; } = new();
    }

    public class ValueSetConceptDefinition
    {
        public string Code { get; set; } = string.Empty;

        public string? Display { get; set; }
    }
}