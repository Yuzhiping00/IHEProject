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

        // Map from DTO to FHIR
        public static FHIRPatient ToFhir(PatientModel model)
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


        // Map from FHIR to DB Entity

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





        //public static Patient ToEntity(FHIRPatient fhirPatient, string rawJson)
        //{
        //    return new Patient
        //    {
        //        FhirId = fhirPatient.Id ?? Guid.NewGuid().ToString(),
        //        FamilyName = fhirPatient.Name?.FirstOrDefault()?.Family,
        //        GivenName = fhirPatient.Name?.FirstOrDefault()?.Given?.FirstOrDefault(),
        //        Gender = fhirPatient.Gender?.ToString(),
        //        BirthDate = fhirPatient.BirthDateElement?.ToDateTimeOffset()?.DateTime,
        //        JsonData = rawJson

        //    };
        //}

        //public static FHIRPatient ToFhir(Patient entity)
        //{
        //    var patient = new FHIRPatient
        //    {
        //        Id = entity.FhirId,
        //        Gender = Enum.TryParse<AdministrativeGender>(entity.Gender, true, out var g)
        //            ? g
        //            : (AdministrativeGender?) null,
        //    };

        //    if (entity.FamilyName != null || entity.GivenName != null)
        //    {
        //        patient.Name = new List<HumanName>
        //        {
        //            new HumanName
        //            {
        //                Family = entity.FamilyName,
        //                Given = new[] {entity.GivenName}
        //            }
        //        };
        //    }


        //    if (entity.BirthDate.HasValue)
        //    {

        //        patient.BirthDate = entity.BirthDate.Value.ToString("yyyy-MM-dd");

        //    }

        //    return patient;
        //}

    }
}


