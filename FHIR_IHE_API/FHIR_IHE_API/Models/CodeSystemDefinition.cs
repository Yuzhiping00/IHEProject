namespace FHIR_IHE_API.Models
{
    public class CodeSystemDefinition
    {
        public string Url { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Version { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Status { get; set; } = "active";
        public string Content { get; set; } = "complete";

        public List<CodeSystemConceptDefinition> Concepts { get; set; } = new();
    }

    public class CodeSystemConceptDefinition
    {
        public string Code { get; set; } = string.Empty;

        public string Display { get; set; } = string.Empty;
    }
}
