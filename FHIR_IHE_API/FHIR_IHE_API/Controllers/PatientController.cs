using System.Text;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace FHIR_IHE_API.Controllers
{
    [ApiController]
    [Route("api/patient")]
    public class PatientController : ControllerBase
    {
        // Injecting FhirClient through constructor
        private readonly FhirClient _client;

        private readonly FhirJsonParser _fhirJsonParser;

        public PatientController(FhirClient client)
        {
            _client = client; // _client is now injected
            _fhirJsonParser = new FhirJsonParser();
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreatePatient()
        {
            try
            {
                using var reader = new StreamReader((Request.Body), Encoding.UTF8);
                var json = await reader.ReadToEndAsync();

                var patient = _fhirJsonParser.Parse<Patient>(json);
                if (patient == null)
                {
                    return BadRequest("Invalid FHIR patient Json");
                }

                var creatdPatient = await _client.CreateAsync(patient);
                return CreatedAtAction(nameof(GetPatientById), new {id = creatdPatient?.Id}, creatdPatient);
            }

            catch (FhirOperationException ex)
            {
                // handle FHIR specific exceptions
                return BadRequest($"FHIR error: {ex.Message} ");
            }

            catch (SystemException ex)
            {
                // handle general errors
                return StatusCode(500, $"Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPatientById(string id)
        {
            try
            {
                var patient = await _client.ReadAsync<Patient>($"Patient/{id}");

                return Ok(patient);
            }
            catch (FhirOperationException ex)
            {
                return NotFound($"FHiR error: {ex.Message}");
            }
        }

        [HttpGet("search")]
        public IActionResult SearchPatients([FromQuery] string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return BadRequest("Name is required.");
            }
            
            var result = _client.Search<Patient>(new string[] {$"name={name}"});

            var patients = result?.Entry
                .Where(e => e.Resource is Patient)
                .Select(e => (Patient) e.Resource)
                .Select(p => new
                {
                    p.Id,
                    Name = p.Name.FirstOrDefault()?.ToString(),
                    p.Gender,
                    p.BirthDate

                }).ToList();

            if (patients == null || !patients.Any())
            {
                return NotFound("There is no patient matching the name");
            }

            return Ok(patients);

        }
    }
}
