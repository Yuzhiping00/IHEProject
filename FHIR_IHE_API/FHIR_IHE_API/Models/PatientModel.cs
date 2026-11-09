
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace FHIR_IHE_API.Models
{
    public class PatientModel : IValidatableObject
    {
        public string ResourceType { get; set; } = "Patient";

        // The server generates the FHIR resource ID.
        public string? Id { get; set; }

        [Required]
        [MinLength(1)]
        public List<NameModel> Name { get; set; } = new();

        [Required]
        public string? Gender { get; set; }

        [Required]
        public string? BirthDate { get; set; }

        public IEnumerable<ValidationResult> Validate(
            ValidationContext validationContext)
        {
            // Validate the FHIR resource type.
            if (!string.Equals(
                ResourceType,
                "Patient",
                StringComparison.Ordinal))
            {
                yield return new ValidationResult(
                    "ResourceType must be Patient.",
                    new[] { nameof(ResourceType) });
            }

            // Validate FHIR AdministrativeGender values.
            var allowedGenders = new[]
            {
                "male",
                "female",
                "other",
                "unknown"
            };

            if (!string.IsNullOrWhiteSpace(Gender) &&
                !allowedGenders.Contains(
                    Gender,
                    StringComparer.Ordinal))
            {
                yield return new ValidationResult(
                    "Gender must be male, female, other, or unknown.",
                    new[] { nameof(Gender) });
            }

            // Validate FHIR date format: yyyy-MM-dd.
            if (!string.IsNullOrWhiteSpace(BirthDate) &&
                !DateTime.TryParseExact(
                    BirthDate,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out _))
            {
                yield return new ValidationResult(
                    "BirthDate must use the yyyy-MM-dd format.",
                    new[] { nameof(BirthDate) });
            }
        }
    }

    public class NameModel : IValidatableObject
    {
        [Required]
        public string? Family { get; set; }

        [Required]
        [MinLength(1)]
        public List<string> Given { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(
            ValidationContext validationContext)
        {
            if (Given != null &&
                Given.Any(string.IsNullOrWhiteSpace))
            {
                yield return new ValidationResult(
                    "Given names cannot contain empty values.",
                    new[] { nameof(Given) });
            }
        }
    }
}
