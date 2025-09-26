using System.Text.Json;
using FHIR_IHE_API.Data;
using FHIR_IHE_API.Mapper;
using FHIR_IHE_API.Models;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FHIRPatient = Hl7.Fhir.Model.Patient;
using Patient = FHIR_IHE_API.Models.Patient;

namespace FHIR_IHE_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PatientController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly FhirJsonParser  _parser = new FhirJsonParser();
        private readonly FhirJsonSerializer _serializer = new FhirJsonSerializer();
        private readonly ILogger<PatientController> _logger;

        // Injecting database in constructor
        public PatientController(ApplicationDbContext context, ILogger<PatientController> logger)
        {
           _context = context;
           _logger = logger;
        }

        //GET: api/patient
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Patient>>> GetPatients()
        {
            var patients = await _context.Patients.ToListAsync();

            if (!patients.Any())
            {
                return NotFound("No patient has been found!");
            }

            return patients;
        }

        //POST: api/patient/create
        //[HttpPost("create")]
        //public async Task<ActionResult<Patient>> CreatePatient([FromBody]Patient patient)
        //{
        //   _context.Patients.Add(patient);
        //   try
        //   {
        //       await _context.SaveChangesAsync();
        //       return patient;
        //   }
        //   catch (Exception ex)
        //   {
        //       return BadRequest(ex.Message);
        //   }

        //}


        [HttpPost("create")]
        public async Task<ActionResult> CreatePatient([FromBody]PatientModel patientModel)
        {

            try
            {
                // 1. Map DTO -> FHIR
                var fhirPatient = PatientMapper.ToFhir(patientModel);

                // 2. Map FHIR -> Entity 
                var entity = PatientMapper.ToEntity(fhirPatient);

                // 3. Save
                _context.Patients.Add(entity);
                await _context.SaveChangesAsync();

                // 4. Return FHIR JSON back
                return Ok(fhirPatient);
            }
            catch (Exception ex)
            {
               // return BadRequest(ex.Message);
               return BadRequest(new { error = ex.Message });
            }

        }

        //GET: api/patient/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Patient>> GetPatient(int id)
        {
           var patient = await _context.Patients.FindAsync(id);

           if (patient == null)
           {
               return NotFound();
           }

           return patient;
        }

        // PUT: api/patient/update
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPatient(int id, [FromBody] Patient updatedPatient)
        {
            if (id != updatedPatient.Id)
            {
                return BadRequest();
            }
            var existingPatient = await _context.Patients.FindAsync(id);
            if (existingPatient == null)
            {
                return NotFound();
            }

            if (updatedPatient.Gender != existingPatient.Gender)
            {
                existingPatient.Gender = updatedPatient.Gender;
            }

            if (updatedPatient.BirthDate != existingPatient.BirthDate)
            {
                existingPatient.BirthDate = updatedPatient.BirthDate;
            }

            if (updatedPatient.GivenName != existingPatient.GivenName)
            {
                existingPatient.GivenName = updatedPatient.GivenName;
            }

            if (updatedPatient.FamilyName != existingPatient.FamilyName)
            {
                existingPatient.FamilyName = updatedPatient.FamilyName;
            }


            await _context.SaveChangesAsync();
            return Ok();
        }

        //DELETE: api/patient/5
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeletePatient(int id)
        {
            var patient = await _context.Patients.FindAsync(id);

            if (patient == null)
            {
                return NotFound();
            }

            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();

            return Ok("The patient has been deleted successfully.");
        }

    }
}
