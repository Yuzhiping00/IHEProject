using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace FHIR_IHE_API.Models
{
    public class FhirResult : ContentResult
    {
        public FhirResult(Resource resource)
        {
            var serializer = new FhirJsonSerializer(new SerializerSettings()
            {
                Pretty = true // optional : makes JSON pretty - printed
            });

            Content = serializer.SerializeToString(resource);
            ContentType = "application/fhir+json";
            StatusCode = 200;

        }
    }
}
