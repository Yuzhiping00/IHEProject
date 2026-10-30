using FHIR_IHE_API.Models;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Support;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Globalization;
using System.Text.Json;
using FHIRPatient = Hl7.Fhir.Model.Patient;
using Patient = FHIR_IHE_API.Models.Patient;


namespace FHIR_IHE_API.Mapper
{
    public class PatientMapper
    {
        private static readonly FhirJsonSerializer _serializer = new FhirJsonSerializer();

        // Map Vue PatientModel to FHIR Patient

        public static FHIRPatient ToFhirFromModel(PatientModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            if (string.IsNullOrWhiteSpace(model.Gender) ||
                !Enum.TryParse<AdministrativeGender>(
                    model.Gender,
                    true,
                    out var gender) ||
                !Enum.IsDefined(typeof(AdministrativeGender), gender))
            {
                throw new ArgumentException(
                    "Invalid FHIR Patient gender.",
                    nameof(model));
            }

            if (string.IsNullOrWhiteSpace(model.BirthDate) ||
                !DateTime.TryParseExact(
                    model.BirthDate,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out _))
            {
                throw new ArgumentException(
                    "BirthDate must use the yyyy-MM-dd format.",
                    nameof(model));
            }

            var fhirPatient = new FHIRPatient
            {
                Id = Guid.NewGuid().ToString(),
                Gender = gender,
                BirthDate = model.BirthDate
            };

            if (model.Name?.Any() == true)
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
            ArgumentNullException.ThrowIfNull(fhirPatient);

            return new Patient
            {
                FhirId = fhirPatient.Id,
                FamilyName = fhirPatient.Name?.FirstOrDefault()?.Family,
                GivenName = fhirPatient.Name?.FirstOrDefault()?.Given?.FirstOrDefault(),
                Gender = fhirPatient.Gender?.ToString(),
                BirthDate = fhirPatient.BirthDateElement?.ToDateTimeOffset()?.Date,
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


        public static Bundle ToBundle(IEnumerable<Patient> entities)
        {
            var patients = entities.Select(ToFhirFromEntity).ToList();
            var bundle = new Bundle
            {
                Type = Bundle.BundleType.Searchset,
                Total = patients.Count,
                Entry = patients.Select(p => new Bundle.EntryComponent
                {
                    Resource =p
                }).ToList()

            };

            return bundle;

        }
    }
}


