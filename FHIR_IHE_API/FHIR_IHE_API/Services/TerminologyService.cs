using FHIR_IHE_API.Models;
using Hl7.Fhir.Model;

namespace FHIR_IHE_API.Services
{
    public class TerminologyService
    {
        private readonly List<CodeSystemDefinition> _codeSystems;
        private readonly List<ValueSetDefinition> _valueSets;

        public TerminologyService()
        {
            _codeSystems = new List<CodeSystemDefinition>
            {
                CreateAdministrativeGenderCodeSystem()
            };

            _valueSets = new List<ValueSetDefinition>
            {
                CreateAdministrativeGenderValueSet()
            };

        }

        private static ValueSetDefinition CreateAdministrativeGenderValueSet()
        {
            return new ValueSetDefinition
            {
                Id = "administrative-gender",
                Url = "http://hl7.org/fhir/ValueSet/administrative-gender",
                Name = "AdministrativeGender",
                Version = "4.0.1",
                Title = "Administrative Gender",
                Status = "active",
                //这里的 Includes 表示 ValueSet 从哪个 CodeSystem 选取 codes。
                //目前我们没有在 Concepts 中指定个别 codes，因此这项 Include 表示包含整个 Administrative Gender CodeSystem。
                Includes =
                {
                    new ValueSetIncludeDefinition
                    {
                        System = "http://hl7.org/fhir/administrative-gender"
                    }
                }
            };
        }

        // compose.include  描述如何选择 codes

        // expansion.contains  展示实际展开后的 codes

        public ValueSet ExpandValueSet(string valueSetId)
        {
            var definition = GetValueSetById(valueSetId);

            if (definition == null)
            {
                throw new KeyNotFoundException(
                    $"ValueSet '{valueSetId}' was not found.");
            }

            // Create the FHIR ValueSet resource.
            var valueSet = ToFhirValueSet(definition);

            var expandedConcepts = new List<ValueSet.ContainsComponent>();

            foreach (var include in definition.Includes)
            {
                var codeSystem = GetCodeSystem(include.System);

                if (codeSystem == null ||
                    !CanValidateCode(include.System))
                {
                    continue;
                }

                // An empty concept list means the entire CodeSystem is included.
                if (include.Concepts.Count == 0)
                {
                    foreach (var concept in codeSystem.Concepts)
                    {
                        expandedConcepts.Add(
                            new ValueSet.ContainsComponent
                            {
                                System = include.System,
                                Code = concept.Code,
                                Display = concept.Display
                            });
                    }
                }
                else
                {
                    // Only explicitly selected concepts are included.
                    foreach (var selectedConcept in include.Concepts)
                    {
                        var concept = FindCode(
                            include.System,
                            selectedConcept.Code);

                        if (concept == null)
                        {
                            continue;
                        }

                        expandedConcepts.Add(
                            new ValueSet.ContainsComponent
                            {
                                System = include.System,
                                Code = concept.Code,
                                Display = selectedConcept.Display
                                    ?? concept.Display
                            });
                    }
                }
            }

            // Remove duplicate System + Code combinations.
            var uniqueConcepts = expandedConcepts
                .GroupBy(x => $"{x.System}|{x.Code}")
                .Select(x => x.First())
                .ToList();

            valueSet.Expansion = new ValueSet.ExpansionComponent
            {
                Total = uniqueConcepts.Count,
                Contains = uniqueConcepts
            };

            return valueSet;
        }


        public bool IsCodeInValueSet(string valueSetId, string system, string code)
        {
            if (string.IsNullOrWhiteSpace(valueSetId) ||
                string.IsNullOrWhiteSpace(system) ||
                string.IsNullOrWhiteSpace(code))
            {
                return false;
            }

            var valueSet = GetValueSetById(valueSetId);

            if (valueSet == null)
            {
                return false;
            }

            // Find includes that reference the requested CodeSystem.
            var includes = valueSet.Includes
                .Where(x => string.Equals(
                    x.System,
                    system,
                    StringComparison.Ordinal))
                .ToList();

            if (includes.Count == 0)
            {
                return false;
            }

            // The code must exist in a CodeSystem that can be validated.
            if (!CanValidateCode(system))
            {
                return false;
            }

            var systemConcept = FindCode(system, code);

            if (systemConcept == null)
            {
                return false;
            }

            // Check whether the ValueSet includes the entire CodeSystem
            // or only explicitly selected concepts.
            foreach (var include in includes)
            {
                if (include.Concepts.Count == 0)
                {
                    return true;
                }

                var isIncluded = include.Concepts.Any(
                    x => string.Equals(
                        x.Code,
                        code,
                        StringComparison.Ordinal));

                if (isIncluded)
                {
                    return true;
                }
            }

            return false;
        }


        public ValueSetCodeValidationResult ValidateValueSetCode(string valueSetId, string system, string code,
            string? display = null)
        {
            var valueSet = GetValueSetById(valueSetId);

            if (valueSet == null)
            {
                return new ValueSetCodeValidationResult
                {
                    IsValid = false,
                    ValueSetFound = false,
                    System = system,
                    Code = code,
                    ProvidedDisplay = display,
                    Message = $"ValueSet '{valueSetId}' was not found."
                };
            }

            var codeSystem = GetCodeSystem(system);

            if (codeSystem == null)
            {
                return new ValueSetCodeValidationResult
                {
                    IsValid = false,
                    ValueSetFound = true,
                    SystemFound = false,
                    System = system,
                    Code = code,
                    ProvidedDisplay = display,
                    Message = $"CodeSystem '{system}' was not found."
                };
            }

            if (!CanValidateCode(system))
            {
                return new ValueSetCodeValidationResult
                {
                    IsValid = false,
                    ValueSetFound = true,
                    SystemFound = true,
                    System = system,
                    Code = code,
                    ProvidedDisplay = display,
                    Message = $"CodeSystem '{system}' cannot be fully validated."
                };
            }

            var concept = FindCode(system, code);

            if (concept == null)
            {
                return new ValueSetCodeValidationResult
                {
                    IsValid = false,
                    ValueSetFound = true,
                    SystemFound = true,
                    System = system,
                    Code = code,
                    ProvidedDisplay = display,
                    Message = $"Code '{code}' was not found in CodeSystem '{system}'."
                };
            }

            var isInValueSet = IsCodeInValueSet(valueSetId, system, code);

            if (!isInValueSet)
            {
                return new ValueSetCodeValidationResult
                {
                    IsValid = false,
                    ValueSetFound = true,
                    SystemFound = true,
                    System = system,
                    Code = code,
                    ProvidedDisplay = display,
                    ExpectedDisplay = concept.Display,
                    Message = $"Code '{code}' is not included in ValueSet '{valueSetId}'."
                };
            }

            if (!string.IsNullOrWhiteSpace(display) && !string.Equals(display, concept.Display, StringComparison.Ordinal))
            {
                return new ValueSetCodeValidationResult
                {
                    IsValid = false,
                    ValueSetFound = true,
                    SystemFound = true,
                    System = system,
                    Code = code,
                    ProvidedDisplay = display,
                    ExpectedDisplay = concept.Display,
                    Message = $"Display does not match the CodeSystem definition."
                };
            }

            return new ValueSetCodeValidationResult
            {
                IsValid = true,
                ValueSetFound = true,
                SystemFound = true,
                System = system,
                Code = code,
                ProvidedDisplay = display,
                ExpectedDisplay = concept.Display
            };

        }

        public ValueSetDefinition? GetValueSetById(string id)
        {
            return _valueSets.FirstOrDefault(x => x.Id.Equals(id, StringComparison.Ordinal));
        }

        public ValueSetDefinition? GetValueSet(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            return _valueSets.FirstOrDefault(x => x.Url.Equals(url, StringComparison.OrdinalIgnoreCase));
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

        public ValueSet ToFhirValueSet(ValueSetDefinition definition)
        {
            var valueSet = new ValueSet
            {
                Url = definition.Url,
                Name = definition.Name,
                Version = definition.Version,
                Title = definition.Title,
                Status = ParsePublicationStatus(definition.Status),

                Compose = new ValueSet.ComposeComponent
                {
                    Include = definition.Includes
                        .Select(include => new ValueSet.ConceptSetComponent
                        {
                            System = include.System,
                            Version = include.Version,

                            Concept = include.Concepts
                                .Select(concept =>
                                    new ValueSet.ConceptReferenceComponent
                                    {
                                        Code = concept.Code,
                                        Display = concept.Display
                                    })
                                .ToList()
                        })
                        .ToList()
                }
            };

            return valueSet;
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
