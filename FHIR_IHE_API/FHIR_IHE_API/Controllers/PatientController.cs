using FHIR_IHE_API.Data;
using FHIR_IHE_API.Mapper;
using FHIR_IHE_API.Models;
using FHIR_IHE_API.Services;
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
        private readonly ILogger<PatientController> _logger;
        private readonly PatientAuthorizationService _patientAuthorizationService;
        private readonly AuditService _auditService;

        // Injecting database in constructor
        public PatientController(ApplicationDbContext context, ILogger<PatientController> logger, PatientAuthorizationService patientAuthorizationService, AuditService auditService)
        {
            _context = context;
            _logger = logger;
            _patientAuthorizationService = patientAuthorizationService;
            _auditService = auditService;
        }


        //GET: api/patient
        [Authorize(Policy = "ProviderOnly")]
        [HttpGet]
        [HttpGet("/Patient")]
        [Produces("application/fhir+json")]
        public async Task<IActionResult> GetAllPatients()
        {
            var entities = await _context.Patients
                .AsNoTracking()
                .OrderBy(p => p.FhirId)
                .ToListAsync();

            var bundle = new Bundle
            {
                Type = Bundle.BundleType.Searchset,
                Total = entities.Count,
                Entry = new List<Bundle.EntryComponent>()
            }; 

            foreach(var entity in entities)
            {
                FHIRPatient fhirPatient;

                try
                {
                    fhirPatient = ParsePatient(entity);
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Failed to parse Patient JSON, Patient ID: {Id}", entity.FhirId);

                    return StatusCode(StatusCodes.Status500InternalServerError);
                }

                var fullUrl = $"{Request.Scheme}://{Request.Host}" + $"{Request.PathBase}/Patient/" + Uri.EscapeDataString(entity.FhirId);

                bundle.Entry.Add(new Bundle.EntryComponent
                {
                    FullUrl = fullUrl,
                    Resource = fhirPatient
                });

            }

            await _auditService.LogAsync(HttpContext, "READ", "Patient", null, StatusCodes.Status200OK);

            return new FhirResult(bundle);
        }

        [Authorize(Policy = "ProviderOnly")]
        [HttpPost("create")]
        [Produces("application/fhir+json")]
        public async Task<ActionResult> CreatePatient([FromBody] PatientModel patientModel)
        {
            try
            {
                // 1. Map request model to FHIR Patient.
                var fhirPatient =PatientMapper.ToFhirFromModel(patientModel);

                // 2. Map FHIR Patient to database entity.
                var entity = PatientMapper.ToEntity(fhirPatient);

                // 3. Save Patient to database.
                _context.Patients.Add(entity);
                await _context.SaveChangesAsync();

                // 4. Record successful creation.
                await _auditService.LogPatientAccessAsync(
                    HttpContext,
                    "CREATE",
                    entity.Id,
                    entity.FhirId,
                    StatusCodes.Status201Created);

                // 5. Build the FHIR resource location.
                var location =
                    $"{Request.Scheme}://{Request.Host}" +
                    $"{Request.PathBase}/Patient/" +
                    Uri.EscapeDataString(entity.FhirId!);

                // 6. Return HTTP 201 Created.
                Response.Headers.Location = location;

                return new FhirResult(fhirPatient, StatusCodes.Status201Created);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to create Patient resource.");

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { error = "An error occurred while creating the Patient resource." });
            }
        }


        // GET: api/patient/10ea202e-5787-46b3-8ef0-377963babfad
        // patient a can only access their own data, not patient b's. => ownership / resource authorization
        [Authorize(Policy = "ProviderOrPatient")]
        [HttpGet("{id}")]
        [HttpGet("/Patient/{id}")]
        [Produces("application/fhir+json")]
        public async Task<IActionResult> GetPatient(string id)
        {
            var entity = await _context.Patients
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.FhirId == id);

            if (entity == null)
            {
                return NotFound();
            }

            var canAccess = _patientAuthorizationService.CanAccessPatient(entity.Id);

            if (!canAccess)
            {
                await _auditService.LogPatientAccessAsync(HttpContext, "READ", entity.Id, entity.FhirId, StatusCodes.Status403Forbidden);
                return Forbid();
            }

            FHIRPatient fhirPatient;

            try
            {
                fhirPatient = ParsePatient(entity);
            }
            catch (Exception ex)
            {
               _logger.LogError(
               ex,
               "Failed to restore FHIR Patient. Patient ID: {Id}",
               entity.FhirId);

                return StatusCode(
                    StatusCodes.Status500InternalServerError);
            }


            await _auditService.LogPatientAccessAsync(HttpContext, "READ", entity.Id, entity.FhirId,
                StatusCodes.Status200OK);

            return new FhirResult(fhirPatient);
        }

        // PUT: api/patient/id/update
        [Authorize(Policy = "ProviderOnly")]
        [HttpPut("{id}/update")]
        [Produces("application/fhir+json")]
        public async Task<IActionResult> PutPatient(string id, [FromBody] PatientModel? updatedPatient)
        {
            // 1. validate the route ID
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest();
            }

            // 2. validate the request body
            if (updatedPatient == null)
            {
                return BadRequest(new OperationOutcome
                {
                    Issue = new List<OperationOutcome.IssueComponent>
                    {
                        new()
                        {
                            Severity = OperationOutcome.IssueSeverity.Error,
                            Code = OperationOutcome.IssueType.Invalid,
                            Diagnostics = "Invalid Patient Payload."
                        }
                    }
                });
            }

            // 2. find the existing patient

            var entity = await _context.Patients.FirstOrDefaultAsync(p => p.FhirId == id);

            if (entity == null)
            {
                return NotFound();
            }

            try
            {
                // 4. update the existing patient
                PatientMapper.UpdateEntity(entity, updatedPatient, id);

                // 5. save changes to the database
                await _context.SaveChangesAsync();

                // 6. restore the updated FHIR Patient from the entity
                var parser = new FhirJsonParser();

                var fhirPatient = parser.Parse<FHIRPatient>(entity.JsonData!);

                // 7. record the successful update
                await _auditService.LogPatientAccessAsync(HttpContext, "UPDATE", entity.Id, entity.FhirId,
                    StatusCodes.Status200OK);

                // 8. return the updated FHIR Patient
                return new FhirResult(fhirPatient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update Patient resource. Patient ID: {Id}",id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { error = "An error occurred while updating the Patient resource." });
            }
        }

        //DELETE: api/patient/5
        [Authorize(Policy = "ProviderOnly")]
        [HttpDelete("delete/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeletePatient(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest();
            }

            var entity = await _context.Patients.FirstOrDefaultAsync(p => p.FhirId == id);

            if (entity == null)
            {
                return NotFound();
            }

            try
            {
                // Preserve audit information before deleting the entity.
                var patientDbId = entity.Id;
                var patientFhirId = entity.FhirId;

                _context.Patients.Remove(entity);

                await _context.SaveChangesAsync();

                await _auditService.LogPatientAccessAsync(
                    HttpContext,
                    "DELETE",
                    patientDbId,
                    patientFhirId,
                    StatusCodes.Status204NoContent);

                return NoContent();
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete Patient resource. Patient ID: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { error = "An error occurred while deleting the Patient resource." });
            }

        }

        [Authorize(Policy = "PatientOnly")]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyPatient()
        {
            var patientId = _patientAuthorizationService.GetCurrentPatientId();

            if (!patientId.HasValue)
            {
                return Forbid();
            }

            var entity = await _context.Patients
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == patientId.Value);

            if (entity == null)
            {
                return NotFound();
            }

            FHIRPatient fhirPatient;

            try
            {
                fhirPatient = ParsePatient(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Failed to restore current user's FHIR Patient.");

                await _auditService.LogPatientAccessAsync(
                HttpContext,
                "READ",
                entity.Id,
                entity.FhirId,
                StatusCodes.Status500InternalServerError);

                return StatusCode(
                    StatusCodes.Status500InternalServerError);
            }

            await _auditService.LogPatientAccessAsync(
                HttpContext,
                "READ",
                entity.Id,
                entity.FhirId,
                StatusCodes.Status200OK);

            return new FhirResult(fhirPatient);
        }

        private FHIRPatient ParsePatient(Models.Patient entity)
        {
            if(!string.IsNullOrWhiteSpace(entity.JsonData))
            {
                return _parser.Parse<FHIRPatient>(entity.JsonData);
            }

            return PatientMapper.ToFhirFromEntity(entity);
        }

    }
}
