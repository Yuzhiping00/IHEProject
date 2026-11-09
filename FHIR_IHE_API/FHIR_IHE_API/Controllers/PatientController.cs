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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Globalization;

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

        [AllowAnonymous]
        [HttpGet("/api/metadata")]
        [Produces("application/fhir+json")]
        [ProducesResponseType(StatusCodes.Status200OK)]

        public IActionResult GetCapabilityStatement()
        {
            // Build the canonical FHIR server base URL.
            var fhirBaseUrl = GetFhirBaseUrl();

            // Describe the FHIR capabilities currently implemented by this server.
            var capabilityStatementJson = new
            {
                resourceType = "CapabilityStatement",

                status = "active",

                date = DateTime.UtcNow.ToString("yyyy-MM-dd"),

                kind = "instance",

                software = new
                {
                    name = "FHIR_IHE_API",
                    version = "1.0.0"
                },

                implementation = new
                {
                    description = "FHIR_IHE_API FHIR R4 Server",
                    url = fhirBaseUrl
                },

                fhirVersion = "4.0.1",

                format = new[]
                {
                    "json"
                },

                rest = new[]
                {
                    new
                    {
                        mode = "server",

                        resource = new[]
                        {
                            new
                            {
                                type = "Patient",

                                interaction = new[]
                                {
                                    new { code = "read" },
                                    new { code = "search-type" },
                                    new { code = "create" },
                                    new { code = "update" },
                                    new { code = "delete" }
                                }
                            }
                        }
                    }
                }
            };

            // Convert the capability statement JSON into a FHIR resource.
            var json = JsonConvert.SerializeObject(capabilityStatementJson);

            var capabilityStatement = _parser.Parse<CapabilityStatement>(json);

            return new FhirResult(capabilityStatement);
        }

        //GET: api/patient
        [Authorize(Policy = "ProviderOnly")]
        [HttpGet]
        [HttpGet("/Patient")]
        [Produces("application/fhir+json")]
        public async Task<IActionResult> GetAllPatients(
            [FromQuery(Name = "_id")] string? fhirId, 
            [FromQuery(Name = "name")] string? name,
            [FromQuery(Name = "gender")] string? gender,
            [FromQuery(Name = "birthdate")] string? birthdate,
            [FromQuery(Name = "_count")] string? count,
            [FromQuery(Name = "_page")] string? page
        )
        {
            // ----------------------------------------
            // 1. Validate supported search parameters
            // ----------------------------------------

            var supportedParameters = new HashSet<string>(
                new[] { "_id", "name", "gender", "birthdate",  "_count", "_page"},
                StringComparer.OrdinalIgnoreCase);

            var unsupportedParameter = Request.Query.Keys
                .FirstOrDefault(key => !supportedParameters.Contains(key));

            if (unsupportedParameter != null)
            {
                return CreateFhirOperationOutcome(
                    $"Search parameter '{unsupportedParameter}' is not supported.",
                    OperationOutcome.IssueType.NotSupported,
                    StatusCodes.Status400BadRequest);
            }

            // ----------------------------------------
            // 2. Read search parameter values
            // ----------------------------------------

            if (!TryGetSearchParameter("_id", fhirId, out fhirId, out var error))
            {
                return CreateFhirOperationOutcome(
                    error!,
                    OperationOutcome.IssueType.Invalid,
                    StatusCodes.Status400BadRequest);
            }

            if (!TryGetSearchParameter("name", name, out name, out error))
            {
                return CreateFhirOperationOutcome(
                    error!,
                    OperationOutcome.IssueType.Invalid,
                    StatusCodes.Status400BadRequest);
            }

            if (!TryGetSearchParameter("gender", gender, out gender, out error))
            {
                return CreateFhirOperationOutcome(
                    error!,
                    OperationOutcome.IssueType.Invalid,
                    StatusCodes.Status400BadRequest);
            }

            if (!TryGetSearchParameter("birthdate", birthdate, out birthdate, out error))
            {
                return CreateFhirOperationOutcome(
                    error!,
                    OperationOutcome.IssueType.Invalid,
                    StatusCodes.Status400BadRequest);
            }

            if (!TryGetSearchParameter("_count", count, out count, out error))
            {
                return CreateFhirOperationOutcome(
                    error!,
                    OperationOutcome.IssueType.Invalid,
                    StatusCodes.Status400BadRequest);
            }

            if (!TryGetSearchParameter("_page", page, out page, out error))
            {
                return CreateFhirOperationOutcome(
                    error!,
                    OperationOutcome.IssueType.Invalid,
                    StatusCodes.Status400BadRequest);
            }

            // ----------------------------------------
            // 3. Validate FHIR ID
            // ----------------------------------------
            if (fhirId != null &&
                !Regex.IsMatch(fhirId, @"^[A-Za-z0-9.-]{1,64}$"))
            {
                return CreateFhirOperationOutcome(
                    "Invalid FHIR Patient ID.",
                    OperationOutcome.IssueType.Invalid,
                    StatusCodes.Status400BadRequest);
            }

            // ----------------------------------------
            // 4. Validate Gender
            // ----------------------------------------

            var validGenders = new[]
            {
                "male",
                "female",
                "other",
                "unknown"
            };

            if (gender != null && !validGenders.Contains(gender, StringComparer.OrdinalIgnoreCase))
            {
                return CreateFhirOperationOutcome(
                    "Invalid gender. Supported values are male, female, other and unknown.",
                    OperationOutcome.IssueType.Invalid,
                    StatusCodes.Status400BadRequest);
            }

            // ----------------------------------------
            // 5. Validate BirthDate
            // ----------------------------------------

            DateTime? parsedBirthDate = null;


            if (birthdate != null)
            {
                if (!DateTime.TryParseExact(
                        birthdate,
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var date))
                {
                    return CreateFhirOperationOutcome(
                        "Invalid birthdate. Expected format is yyyy-MM-dd.",
                        OperationOutcome.IssueType.Invalid,
                        StatusCodes.Status400BadRequest);
                }

                parsedBirthDate = date.Date;
            }

            // ----------------------------------------
            // 6. Validate Pagination Parameters
            // ----------------------------------------

            const int defaultPageSize = 20;
            const int maximumPageSize = 100;

            var pageSize = defaultPageSize;
            var pageNumber = 1;

            // validate _count
            if (count != null)
            {
                if (!int.TryParse(count, out pageSize) || pageSize < 1 || pageSize > maximumPageSize)
                {
                    return CreateFhirOperationOutcome(
                        $"Invalid _count value. The value must be between 1 and {maximumPageSize}.",
                        OperationOutcome.IssueType.Invalid, StatusCodes.Status400BadRequest);
                }
            }

            // validate _page

            if (page != null)
            {
                if (!int.TryParse(page, out pageNumber) || pageNumber < 1)
                {
                    return CreateFhirOperationOutcome(
                        $"Invalid page value. The value must be a positive integer greater than or equal to {pageNumber}.",
                        OperationOutcome.IssueType.Invalid, StatusCodes.Status400BadRequest);
                }
            }

            // Calculate the number of records to skip.
            var offset = (pageNumber - 1) * pageSize;


            // ----------------------------------------
            // 7. Build database query
            // ----------------------------------------

            IQueryable<Models.Patient> query = _context.Patients.AsNoTracking();

            // Search by FHIR Patient ID.
            if (fhirId != null)
            {
                query = query.Where(p => p.FhirId == fhirId);
            }

            // Search by Family Name or Given Name.
            if (name != null)
            {
                var searchName = name.ToLower();

                query = query.Where(p =>
                    (p.FamilyName != null &&
                     p.FamilyName.ToLower().Contains(searchName))
                    ||
                    (p.GivenName != null &&
                     p.GivenName.ToLower().Contains(searchName)));
            }

            // Search by Gender.
            if (gender != null)
            {
                var searchGender = gender.ToLower();

                query = query.Where(p =>
                    p.Gender != null &&
                    p.Gender.ToLower() == searchGender);
            }

            // Search by BirthDate.
            if (parsedBirthDate.HasValue)
            {
                var startDate = parsedBirthDate.Value;
                var endDate = startDate.AddDays(1);

                query = query.Where(p =>
                    p.BirthDate.HasValue &&
                    p.BirthDate.Value >= startDate &&
                    p.BirthDate.Value < endDate);
            }


            // ----------------------------------------
            // 8. Retrieve total count and paged Patient records
            // ----------------------------------------

            // Count all records that match the search criteria.
            // This value represents the total number of matching resources,
            // not the number of resources returned on the current page.

            var totalCount = await query.CountAsync();

            // Retrieve only the records required for the current page.
            var entities = await query
                .OrderBy(p => p.FhirId)
                .Skip(offset)
                .Take(pageSize)
                .ToListAsync();

            // ----------------------------------------
            // 9. Create FHIR Searchset Bundle
            // ----------------------------------------
            var bundle = new Bundle
            {
                Type = Bundle.BundleType.Searchset,
                Total = totalCount
            };

            // Add the self link for the current search request.
            var selfUrl =
                $"{Request.Scheme}://{Request.Host}" +
                $"{Request.PathBase}{Request.Path}{Request.QueryString}";

            bundle.Link.Add(new Bundle.LinkComponent
            {
                Relation = "self",
                Url = selfUrl
            });

            // Add the next link if there are more records to retrieve.
            if (offset + entities.Count < totalCount)
            {
                var nextPageNumber = pageNumber + 1;

                var nextUrl = BuildPaginationUrl(nextPageNumber, pageSize);
                
                bundle.Link.Add(new Bundle.LinkComponent
                {
                    Relation = "next",
                    Url = nextUrl
                });
            }

            // Add the previous link if this is not the first page.
            if (pageNumber > 1)
            {
                var previousPageNumber = Math.Max(1, pageNumber - 1);

                var previousUrl = BuildPaginationUrl(previousPageNumber, pageSize);

                bundle.Link.Add(new Bundle.LinkComponent
                {
                    Relation = "previous",
                    Url = previousUrl
                });
            }

            // ----------------------------------------
            // 10. Add Patient resources to Bundle
            // ----------------------------------------

            foreach (var entity in entities)
            {
                FHIRPatient fhirPatient;

                try
                {
                    fhirPatient = ParsePatient(entity);
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Failed to parse Patient JSON, Patient ID: {Id}", entity.FhirId);

                    return CreateFhirOperationOutcome(
                        "Failed to parse a Patient resource stored on the server.",
                        OperationOutcome.IssueType.Exception,
                        StatusCodes.Status500InternalServerError);
                }

                var fullUrl = GetFhirPatientUrl(entity.FhirId);

                bundle.Entry.Add(new Bundle.EntryComponent
                {
                    FullUrl = fullUrl,
                    Resource = fhirPatient
                });

            }

            // ----------------------------------------
            // 11. Audit successful search
            // ----------------------------------------

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
                Response.Headers.Location = GetFhirPatientUrl(fhirPatient.Id);

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

        // FHIR R4 Create: POST /api/Patient
        [Authorize(Policy = "ProviderOnly")]
        [HttpPost]
        [Consumes("application/fhir+json", "application/json")]
        [Produces("application/fhir+json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateFhirPatient(
            [FromBody] JObject resourceJson)
        {
            if (resourceJson == null)
            {
                return BadRequest();
            }

            FHIRPatient fhirPatient;


            try
            {
                // Convert the incoming JSON into a FHIR Patient resource.
                string json = resourceJson.ToString(Formatting.None);

                fhirPatient = _parser.Parse<FHIRPatient>(json);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Invalid FHIR Patient resource.");

                var outcome = new OperationOutcome();

                    outcome.Issue.Add(new OperationOutcome.IssueComponent
                    {
                        Severity = OperationOutcome.IssueSeverity.Error,
                        Code = OperationOutcome.IssueType.Invalid,
                        Diagnostics = ex.Message
                    });

                    return new FhirResult(
                        outcome,
                        StatusCodes.Status400BadRequest);
            }

            // Continue with the persistence and audit implementation here.

            try
            {
                // 3. Generate a server-assigned FHIR resource ID.
                fhirPatient.Id = Guid.NewGuid().ToString();

                // 4. Map the FHIR Patient to the database entity.
                var entity = PatientMapper.ToEntity(fhirPatient);

                // 5. Save the Patient.
                _context.Patients.Add(entity);
                await _context.SaveChangesAsync();

                // 6. Record the successful creation.
                await _auditService.LogPatientAccessAsync(
                    HttpContext,
                    "CREATE",
                    entity.Id,
                    entity.FhirId,
                    StatusCodes.Status201Created);

                // 7. Build the resource location.
                Response.Headers.Location = GetFhirPatientUrl(fhirPatient.Id);

                // 8. Return the created FHIR Patient.
                return new FhirResult(
                    fhirPatient,
                    StatusCodes.Status201Created);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to create FHIR Patient resource.");

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        error = "An error occurred while creating the Patient resource."
                    });
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

        // FHIR R4 Update: PUT /api/Patient/{id}
        [Authorize(Policy = "ProviderOnly")]
        [HttpPut("{id}")]
        [Consumes("application/fhir+json", "application/json")]
        [Produces("application/fhir+json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<IActionResult> UpdateFhirPatient(string id, [FromBody] JObject? resourceJson)
        {
            // Validate the URL ID.
            if (string.IsNullOrWhiteSpace(id))
            {
                return CreateFhirOperationOutcome(
                    "Patient ID in the request URL is required.",
                    OperationOutcome.IssueType.Invalid,
                    StatusCodes.Status400BadRequest);
            }

            // Validate the request body.
            if (resourceJson == null)
            {
                return CreateFhirOperationOutcome(
                    "Request body is required.",
                    OperationOutcome.IssueType.Invalid,
                    StatusCodes.Status400BadRequest);
            }

            // Get the FHIR resource ID from the request body.
            string? bodyId = resourceJson.Value<string>("id");

            // The resource ID must match the URL ID.
            if (string.IsNullOrWhiteSpace(bodyId) ||
                !string.Equals(bodyId, id, StringComparison.Ordinal))
            {
                return CreateFhirOperationOutcome(
                    "The Patient resource ID must match the ID in the request URL.",
                    OperationOutcome.IssueType.Invalid,
                    StatusCodes.Status400BadRequest);
            }

            FHIRPatient fhirPatient;

            try
            {
                // Parse the incoming JSON into a FHIR Patient resource.
                string json = resourceJson.ToString(Formatting.None);

                fhirPatient = _parser.Parse<FHIRPatient>(json);
            }

            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to parse FHIR Patient resource. Patient ID: {id}",
                    id);

                return CreateFhirOperationOutcome(
                    "The supplied resource is not a valid FHIR Patient resource.",
                    OperationOutcome.IssueType.Invalid,
                    StatusCodes.Status400BadRequest);
            }

            // Find the existing Patient entity using its FHIR ID.
            var entity = await _context.Patients
                .FirstOrDefaultAsync(p => p.FhirId == id);

            if (entity == null)
            {
                return CreateFhirOperationOutcome(
                    "Patient resource was not found.",
                    OperationOutcome.IssueType.NotFound,
                    StatusCodes.Status404NotFound);
            }
            try
            {
                // Map the complete FHIR Patient resource to an entity.
                var updatedEntity = PatientMapper.ToEntity(fhirPatient);

                // Update the existing database entity.
                // Keep the database primary key and FHIR resource ID unchanged.
                entity.FamilyName = updatedEntity.FamilyName;
                entity.GivenName = updatedEntity.GivenName;
                entity.Gender = updatedEntity.Gender;
                entity.BirthDate = updatedEntity.BirthDate;
                entity.JsonData = updatedEntity.JsonData;

                // Save the updated Patient resource.
                await _context.SaveChangesAsync();

                // Record the successful update in the audit log.
                await _auditService.LogPatientAccessAsync(
                    HttpContext,
                    "UPDATE",
                    entity.Id,
                    entity.FhirId,
                    StatusCodes.Status200OK);

                _logger.LogInformation(
                    "FHIR Patient updated successfully. ID: {PatientId}",
                    id);

                // Return the updated FHIR Patient resource.
                return new FhirResult(
                    fhirPatient,
                    StatusCodes.Status200OK);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to update FHIR Patient. ID: {PatientId}",
                    id);

                return CreateFhirOperationOutcome(
                    "An internal error occurred while updating the Patient resource.",
                    OperationOutcome.IssueType.Exception,
                    StatusCodes.Status500InternalServerError);
            }

        }


        // FHIR R4 Delete: DELETE /api/Patient/{id}
        [Authorize(Policy = "ProviderOnly")]
        [HttpDelete("{id}")]
        [Produces("application/fhir+json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteFhirPatient(string id)
        {
            // Validate the FHIR resource ID.
            if (string.IsNullOrWhiteSpace(id) ||
                !Regex.IsMatch(id, @"^[A-Za-z0-9.-]{1,64}$"))
            {
                return CreateFhirOperationOutcome(
                    "The Patient resource ID must contain 1 to 64 characters and only letters, numbers, hyphens, or periods.",
                    OperationOutcome.IssueType.Invalid,
                    StatusCodes.Status400BadRequest);
            }

            // Find the existing Patient using its FHIR ID.
            var entity = await _context.Patients
                .FirstOrDefaultAsync(p => p.FhirId == id);

            // Return 404 when the Patient does not exist.
            if (entity == null)
            {
                return CreateFhirOperationOutcome(
                    "Patient resource was not found.",
                    OperationOutcome.IssueType.NotFound,
                    StatusCodes.Status404NotFound);
            }

            // Preserve identifiers before deleting the entity.
            var patientDatabaseId = entity.Id;
            var patientFhirId = entity.FhirId;

            try
            {
                // Remove the Patient from the database.
                _context.Patients.Remove(entity);

                await _context.SaveChangesAsync();

                // Record the successful deletion in the audit log.
                await _auditService.LogPatientAccessAsync(
                    HttpContext,
                    "DELETE",
                    patientDatabaseId,
                    patientFhirId,
                    StatusCodes.Status204NoContent);

                _logger.LogInformation(
                    "FHIR Patient deleted successfully. ID: {PatientId}",
                    patientFhirId);

                // FHIR Delete returns 204 with an empty response body.
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to delete FHIR Patient. ID: {PatientId}",
                    patientFhirId);

                return CreateFhirOperationOutcome(
                    "An internal error occurred while deleting the Patient resource.",
                    OperationOutcome.IssueType.Exception,
                    StatusCodes.Status500InternalServerError);
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

        private FhirResult CreateFhirOperationOutcome (string diagnostics, OperationOutcome.IssueType issueType, int statusCode)
        {
            var outcome = new OperationOutcome();

            outcome.Issue.Add(new OperationOutcome.IssueComponent
            {
                Severity = OperationOutcome.IssueSeverity.Error,
                Code = issueType,
                Diagnostics = diagnostics
            });
            return new FhirResult(outcome, statusCode);
        }

        private string GetFhirBaseUrl()
        {
            return $"{Request.Scheme}://{Request.Host}" +
                   $"{Request.PathBase}/api";
        }

        private string GetFhirPatientUrl(string fhirId)
        {
            return $"{GetFhirBaseUrl()}/Patient/" + Uri.EscapeDataString(fhirId);
        }

        // Helper method to extract a single search parameter from the query string.
        private bool TryGetSearchParameter(string parameterName,
            string? boundValue,
            out string? value,
            out string? error)
        {
            value = null;
            error = null;

            if (!Request.Query.TryGetValue(parameterName, out var values))
            {
                return true;
            }

            if (values.Count != 1)
            {
                error = $"Search parameter '{parameterName}' must have exactly one value.";

                return false;
            }

            if (string.IsNullOrWhiteSpace(boundValue))
            {
                error = $"Search parameter '{parameterName}' cannot be empty.";
                return false;
            }

            value = boundValue.Trim();

            return true;
        }

        // Helper method to build pagination URLs for the next and previous pages.
        private string BuildPaginationUrl(int targetPage, int pageSize)
        {
            var queryParameters = new List<string>();

            foreach (var queryParameter in Request.Query)
            {
                var key = queryParameter.Key;

                // Pagination parameters are rebuilt below.
                if (key.Equals("_page", StringComparison.OrdinalIgnoreCase) ||
                    key.Equals("_count", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                foreach (var value in queryParameter.Value)
                {
                    queryParameters.Add(
                        $"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(value)}");
                }
            }

            queryParameters.Add($"_count={pageSize}");
            queryParameters.Add($"_page={targetPage}");

            var queryString = string.Join("&", queryParameters);

            return
                $"{Request.Scheme}://{Request.Host}" +
                $"{Request.PathBase}{Request.Path}?{queryString}";
        }
    }
}
