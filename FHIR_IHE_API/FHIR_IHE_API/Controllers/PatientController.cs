using FHIR_IHE_API.Data;
using FHIR_IHE_API.Mapper;
using FHIR_IHE_API.Models;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FHIRPatient = Hl7.Fhir.Model.Patient;

namespace FHIR_IHE_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PatientController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly FhirJsonParser _parser = new FhirJsonParser();
        private readonly FhirJsonSerializer _serializer = new FhirJsonSerializer();
        private readonly ILogger<PatientController> _logger;

        // Injecting database in constructor
        public PatientController(ApplicationDbContext context, ILogger<PatientController> logger)
        {
            _context = context;
            _logger = logger;
        }


        //GET: api/patient
        [Authorize(Roles = "Provider")]
        [HttpGet]
        public async Task<IActionResult> GetAllPatients()
        {
            var entities = await _context.Patients.ToListAsync();
            var bundle = PatientMapper.ToBundle(entities);

            return new FhirResult(bundle);
        }

        [Authorize(Roles = "Provider")]
        [HttpPost("create")]
        public async Task<ActionResult> CreatePatient([FromBody] PatientModel patientModel)
        {

            try
            {
                // 1. Map DTO -> FHIR
                var fhirPatient = PatientMapper.ToFhirFromModel(patientModel);

                // 2. Map FHIR -> Entity 
                var entity = PatientMapper.ToEntity(fhirPatient);

                // 3. Save
                _context.Patients.Add(entity);
                await _context.SaveChangesAsync();

                // 4. Return FHIR JSON back
                return new FhirResult(fhirPatient); // clean fhir json
            }
            catch (Exception ex)
            {
                // return BadRequest(ex.Message);
                return BadRequest(new { error = ex.Message });
            }

        }

        //GET: api/patient/10ea202e-5787-46b3-8ef0-377963babfad
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPatient(string id)
        {
            var entity = await _context.Patients.FirstOrDefaultAsync(p => p.FhirId == id);

            if (entity == null)
            {
                return NotFound();
            }

            //convert DB entity -> Fhir patient

            var fhirPatient = PatientMapper.ToFhirFromEntity(entity);

            return new FhirResult(fhirPatient);
        }

        // PUT: api/patient/id/update
        [Authorize(Roles = "Provider")]
        [HttpPut("{id}/update")]
        public async Task<IActionResult> PutPatient(string id, [FromBody] PatientModel? updatedPatient)
        {
            if (updatedPatient == null)
            {
                return BadRequest(new OperationOutcome
                {
                    Issue = new List<OperationOutcome.IssueComponent>
                    {
                        new OperationOutcome.IssueComponent
                        {
                            Severity = OperationOutcome.IssueSeverity.Error,
                            Code = OperationOutcome.IssueType.Invalid,
                            Diagnostics = "Invalid Patient Payload."
                        }
                    }
                });
            }

            var entity = await _context.Patients.FirstOrDefaultAsync(p => p.FhirId == id);

            if (entity == null)
            {
                return NotFound();
            }

            PatientMapper.UpdateEntity(entity, updatedPatient, id);

            //var fhirPatient = PatientMapper.ToFhirFromEntity(entity);

            var parser = new FhirJsonParser();
            var fhirPatient = parser.Parse<FHIRPatient>(entity.JsonData);

            await _context.SaveChangesAsync();
            return new FhirResult(fhirPatient);
        }

        //DELETE: api/patient/5
        [Authorize(Roles = "Provider")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeletePatient(string id)
        {
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.FhirId == id);

            if (patient == null)
            {
                return NotFound();
            }

            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}
