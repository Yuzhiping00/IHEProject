using FHIR_IHE_API.Models;
using Hl7.Fhir.Model;

namespace FHIR_IHE_API.Services
{
    public class TerminologyService
    {
        private readonly List<CodeSystemDefinition> _codeSystems;

        public TerminologyService()
        {
            _codeSystems = new List<CodeSystemDefinition>
            {
                CreateAdministrativeGenderCodeSystem()
            };
        }

        public CodeSystemDefinition? GetCodeSystem(string url)
        {
            return _codeSystems.FirstOrDefault(x => x.Url.Equals(url, StringComparison.OrdinalIgnoreCase));
        }

        public bool CanValidateCode(string system)
        {
            var definition = GetCodeSystem(system);

            if (definition == null)
            {
                return false;
            }

            return string.Equals(
                definition.Content,
                "complete",
                StringComparison.OrdinalIgnoreCase);
        }

        public CodeSystemConceptDefinition? FindCode(string system, string code)
        {
            var codeSystem = GetCodeSystem(system);

            if (codeSystem == null)
            {
                return null;
            }

            return codeSystem.Concepts.FirstOrDefault(x => x.Code.Equals(code, StringComparison.Ordinal));
        }

        public CodeValidationResult ValidateCodeDisplay(string system, string code, string? display = null)
        {
            // Find the specified CodeSystem.
            var definition = GetCodeSystem(system);

            if (definition == null)
            {
                return new CodeValidationResult
                {
                    IsValid = false,
                    SystemFound = false,
                    CanValidateCode = false,
                    System = system,
                    Code = code,
                    ProvidedDisplay = display,
                    Message = "The specified CodeSystem was not found."
                };
            }

            // Check whether the CodeSystem contains complete code definitions.
            if (!CanValidateCode(system))
            {
                return new CodeValidationResult
                {
                    IsValid = false,
                    SystemFound = true,
                    CanValidateCode = false,
                    System = system,
                    Code = code,
                    ProvidedDisplay = display,
                    Message = "The CodeSystem does not provide complete code definitions."
                };
            }

            // Look up the code in the CodeSystem.
            var concept = FindCode(system, code);

            if (concept == null)
            {
                return new CodeValidationResult
                {
                    IsValid = false,
                    SystemFound = true,
                    CanValidateCode = true,
                    System = system,
                    Code = code,
                    ProvidedDisplay = display,
                    Message = "The code does not exist in the specified CodeSystem."
                };
            }

            // A display is optional. Validate it only when provided.
            if (string.IsNullOrWhiteSpace(display))
            {
                return new CodeValidationResult
                {
                    IsValid = true,
                    SystemFound = true,
                    CanValidateCode = true,
                    System = system,
                    Code = code,
                    ExpectedDisplay = concept.Display,
                    Message = "The code is valid. No display was provided."
                };
            }

            // Compare the provided display with the CodeSystem display.
            var isDisplayValid = string.Equals(
                display,
                concept.Display,
                StringComparison.Ordinal);

            return new CodeValidationResult
            {
                IsValid = isDisplayValid,
                SystemFound = true,
                CanValidateCode = true,
                System = system,
                Code = code,
                ProvidedDisplay = display,
                ExpectedDisplay = concept.Display,
                Message = isDisplayValid
                    ? "The code and display are valid."
                    : "The provided display does not match the CodeSystem display."
            };
        }

        public CodeValidationResult ValidateCoding(Coding? coding)
        {
            // Reject a null Coding object.
            if (coding == null)
            {
                return new CodeValidationResult
                {
                    IsValid = false,
                    Message = "Coding cannot be null."
                };
            }

            // Validate that the Coding contains a system.
            if (string.IsNullOrWhiteSpace(coding.System))
            {
                return new CodeValidationResult
                {
                    IsValid = false,
                    Code = coding.Code ?? string.Empty,
                    ProvidedDisplay = coding.Display,
                    Message = "Coding must contain a system."
                };
            }

            // Validate that the Coding contains a code.
            if (string.IsNullOrWhiteSpace(coding.Code))
            {
                return new CodeValidationResult
                {
                    IsValid = false,
                    System = coding.System,
                    ProvidedDisplay = coding.Display,
                    Message = "Coding must contain a code."
                };
            }

            // Validate the code and display using the specified CodeSystem.
            return ValidateCodeDisplay(
                coding.System,
                coding.Code,
                coding.Display);
        }

        private static CodeSystemDefinition CreateAdministrativeGenderCodeSystem()
        {
            return new CodeSystemDefinition
            {
                Url = "http://hl7.org/fhir/administrative-gender",
                Name = "AdministrativeGender",
                Version = "4.0.1",
                Title = "Administrative Gender",
                Status = "active",
                Content = "complete",
                Concepts =
                {
                    new CodeSystemConceptDefinition
                    {
                        Code = "male",
                        Display = "Male"
                    },
                    new CodeSystemConceptDefinition
                    {
                        Code = "female",
                        Display = "Female"
                    },
                    new CodeSystemConceptDefinition
                    {
                        Code = "other",
                        Display = "Other"
                    },
                    new CodeSystemConceptDefinition
                    {
                        Code = "unknown",
                        Display = "Unknown"
                    }
                }
            };
        }

        public CodeSystem ToFhirCodeSystem(CodeSystemDefinition definition)
        {
            // Convert the internal definition into a FHIR CodeSystem.
            var codeSystem = new CodeSystem
            {
                Url = definition.Url,
                Name = definition.Name,
                Version = definition.Version,
                Title = definition.Title,
                Status = ParsePublicationStatus(definition.Status),
                Content = ParseCodeSystemContent(definition.Content)
            };

            foreach (var concept in definition.Concepts)
            {
                codeSystem.Concept.Add(
                    new CodeSystem.ConceptDefinitionComponent
                    {
                        Code = concept.Code,
                        Display = concept.Display
                    });
            }

            return codeSystem;
        }

        private static PublicationStatus ParsePublicationStatus(string status)
        {
            return status.ToLowerInvariant() switch
            {
                "active" => PublicationStatus.Active,
                "draft" => PublicationStatus.Draft,
                "retired" => PublicationStatus.Retired,
                _ => PublicationStatus.Unknown
            };
        }

        private static CodeSystemContentMode ParseCodeSystemContent(string content)
        {
            return content.ToLowerInvariant() switch
            {
                "complete" => CodeSystemContentMode.Complete,
                "fragment" => CodeSystemContentMode.Fragment,
                "not-present" => CodeSystemContentMode.NotPresent,
                "example" => CodeSystemContentMode.Example,
                _ => CodeSystemContentMode.NotPresent
            };
        }
    }
}
