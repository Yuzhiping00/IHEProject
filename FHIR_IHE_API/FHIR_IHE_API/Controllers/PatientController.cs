
using System.Reflection.Metadata;
using FHIR_IHE_API.Data;
using FHIR_IHE_API.Models;
using Hl7.Fhir.Rest;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FHIR_IHE_API.Controllers
{
    [Route("api/patient")]
    [ApiController]
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
        [HttpPut("update")]
        public async Task<IActionResult> PutPatient(int id, Patient patient)
        {
            if (id != patient.Id)
            { 
                return BadRequest();
            }

            _context.Entry(patient).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Patients.Any(e => e.Id == id))
                {
                    return NotFound();
                }

                throw;
            }

            return NoContent();
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

            return NoContent();
        }

    }
}
