namespace FHIR_IHE_API.Models
{
    public class PatientModel
    {
        public PatientModel(List<NameModel> name)
        {
            Name = name;
        }

        public string ResourceType { get; set; } = "Patient";
        public string? Id { get; set; }
        public List<NameModel> Name { get; set; }
        public string? Gender { get; set; }
        public string? BirthDate { get; set; }
    }

    public class NameModel
    {
        public NameModel(List<string> given)
        {
            Given = given;
        }

        public string? Family { get; set; }
        public List<string> Given { get; set; }
    }
}
