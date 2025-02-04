using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Microsoft.AspNetCore.Mvc;

namespace FHIR_IHE_API.Controllers
{
    [ApiController]
    [Route("api/pdq")]
    public class PatientController : ControllerBase
    {
        private readonly FhirClient _client = new("http://localhost:8090/fhir");

        [HttpGet("search")]
        public IActionResult SearchPatients([FromQuery] string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return BadRequest("Name is required.");
            }var result = _client.Search<Patient>(new string[] {$"name={name}"});

            var patients = result?.Entry
                .Where(e => e.Resource is Patient)
                .Select(e => (Patient) e.Resource)
                .Select(p => new
                {
                    Id = p.Id,
                    Name = p.Name.FirstOrDefault()?.ToString(),
                    Gender = p.Gender,
                    BirthDate = p.BirthDate

                }).ToList();

            return Ok(patients);

        }
    }
}
