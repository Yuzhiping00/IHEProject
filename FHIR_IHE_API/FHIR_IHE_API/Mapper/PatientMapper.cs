using Hl7.Fhir.Model;
using System.Text.Json;
using FHIR_IHE_API.Models;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Patient = FHIR_IHE_API.Models.Patient;
using FHIRPatient = Hl7.Fhir.Model.Patient;


namespace FHIR_IHE_API.Mapper
{
    public class PatientMapper
    {
        private static readonly FhirJsonSerializer _serializer = new FhirJsonSerializer();

        // Map Vue PatientModel to FHIR Patient
        public static FHIRPatient ToFhirFromModel(PatientModel model)
        {
            var fhirPatient = new FHIRPatient
            {
                Id = string.IsNullOrWhiteSpace(model.Id) ? Guid.NewGuid().ToString() : model.Id,
                Gender = Enum.TryParse<AdministrativeGender>(model.Gender, true, out var g) ? g : null,
                BirthDate = model.BirthDate,
            };

            if (model?.Name.Any() == true)
            {

                fhirPatient.Name = model.Name.Select(n => new HumanName
                {
                    Family = n.Family,
                    Given = n.Given
                }).ToList();
            }

            return fhirPatient;
        }


        // Map from FHIR Patient to DB Entity

        public static Patient ToEntity(FHIRPatient fhirPatient)
        {
            return new Patient
            {
                FhirId = fhirPatient.Id,
                FamilyName = fhirPatient.Name?.FirstOrDefault()?.Family,
                GivenName = fhirPatient.Name?.FirstOrDefault()?.Given?.FirstOrDefault(),
                Gender = fhirPatient.Gender?.ToString(),
                BirthDate = fhirPatient.BirthDateElement?.ToDateTimeOffset()?.DateTime,
                JsonData = _serializer.SerializeToString(fhirPatient)
            };
        }

        public static FHIRPatient ToFhirFromEntity(Patient entity)
        {
            var patient = new FHIRPatient
            {
                Id = entity.FhirId,

                Gender = Enum.TryParse<AdministrativeGender>(entity.Gender, true, out var g)
                    ? g
                    : (AdministrativeGender?) null,

                BirthDate = entity.BirthDate?.ToString("yyyy-MM-dd")
            };

            if (!string.IsNullOrEmpty(entity.FamilyName) || !string.IsNullOrEmpty(entity.GivenName))
            {
                patient.Name = new List<HumanName>
                {
                    new()
                    {
                        Family = entity.FamilyName,
                        Given = string.IsNullOrEmpty(entity.GivenName) ? null : new[] {entity.GivenName}
                    }
                };
            }

            return patient;
        }

        public static void UpdateEntity(Patient entity, PatientModel model, string id)
        {
            var fhirPatient = ToFhirFromModel(model);

            fhirPatient.Id = id;

            entity.FhirId = fhirPatient.Id;
            entity.FamilyName = model.Name?.FirstOrDefault()?.Family;
            entity.GivenName = model.Name?.FirstOrDefault()?.Given.FirstOrDefault();
            entity.Gender = model.Gender;
            entity.BirthDate = DateTime.TryParse(model.BirthDate, out var birthDate)
                ? birthDate 
                : (DateTime?)null;

            entity.JsonData = _serializer.SerializeToString(fhirPatient);
        }
    }
}


