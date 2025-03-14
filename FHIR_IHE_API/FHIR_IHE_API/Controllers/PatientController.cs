
using System.Reflection.Metadata;
using FHIR_IHE_API.Data;
using FHIR_IHE_API.Models;
using Hl7.Fhir.Rest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FHIR_IHE_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PatientController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        // Injecting database in constructor
        public PatientController(ApplicationDbContext context)
        {
           _context = context;
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
        [HttpPost("create")]
        public async Task<ActionResult<Patient>> CreatePatient([FromBody]Patient patient)
        {
           _context.Patients.Add(patient);
           try
           {
               await _context.SaveChangesAsync();
               return patient;
           }
           catch (Exception ex)
           {
               return BadRequest(ex.Message);
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
