using FHIR_IHE_API.Models;
using FHIR_IHE_API.Services;
using Hl7.Fhir.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FHIR_IHE_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CodeSystemController : ControllerBase
    {
        private readonly TerminologyService _terminologyService;

        public CodeSystemController(TerminologyService terminologyService)
        {
            _terminologyService = terminologyService;
        }

        [HttpGet("administrative-gender")]
        [Produces("application/fhir+json")]
        public IActionResult GetAdministrativeGender()
        {
            // Get the Administrative Gender CodeSystem definition

            var definition = _terminologyService.GetCodeSystem("http://hl7.org/fhir/administrative-gender");

            if (definition == null)
            {
                return NotFound();
            }

            // Convert the internal definition into a FHIR CodeSystem.

            var codeSystem = _terminologyService.ToFhirCodeSystem(definition);

            return new FhirResult(codeSystem);
        }

        [HttpGet("administrative-gender/{code}")]
        [Produces("application/json")]
        public IActionResult GetAdministrativeGenderCode(string code)
        {
            // Look up the requested code in the Administrative Gender CodeSystem
            var concept = _terminologyService.FindCode("http://hl7.org/fhir/administrative-gender", code);

            if (concept == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                system = "http://hl7.org/fhir/administrative-gender",
                code = concept.Code,
                display = concept.Display
            });

        }
    }
}
