using FHIR_IHE_API.Models;
using FHIR_IHE_API.Services;
using Hl7.Fhir.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FHIR_IHE_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ValueSetController : ControllerBase
    {
        private readonly TerminologyService _terminologyService;

        public ValueSetController(TerminologyService terminologyService)
        {
            _terminologyService = terminologyService;
        }

        // GET: api/ValueSet/administrative-gender
        [HttpGet("{id}")]
        [Produces("application/fhir+json")]
        public IActionResult GetValueSet(string id)
        {
            if (string.IsNullOrWhiteSpace(id) ||
                !System.Text.RegularExpressions.Regex.IsMatch(
                    id,
                    @"^[A-Za-z0-9.-]{1,64}$"))
            {
                return BadRequest();
            }

            var definition = _terminologyService.GetValueSetById(id);

            if (definition == null)
            {
                return NotFound();
            }

            var valueSet = _terminologyService.ToFhirValueSet(definition);

            valueSet.Id = definition.Id;

            return new FhirResult(valueSet);
        }

        // GET: api/ValueSet/{id}/$expand
        [HttpGet("{id}/$expand")]
        [Produces("application/fhir+json")]
        public IActionResult ExpandValueSet(string id)
        {
            if (string.IsNullOrWhiteSpace(id) ||
                !System.Text.RegularExpressions.Regex.IsMatch(
                    id,
                    @"^[A-Za-z0-9.-]{1,64}$"))
            {
                return BadRequest();
            }

            var definition = _terminologyService.GetValueSetById(id);

            if (definition == null)
            {
                return NotFound();
            }

            var valueSet = _terminologyService.ExpandValueSet(id);

            return new FhirResult(valueSet);
        }

        [HttpGet("{id}/$validate-code")]
        [Produces("application/fhir+json")]
        public IActionResult ValidateCode(string id, [FromQuery] string system, [FromQuery] string code,
            [FromQuery] string? display = null)
        {
            if (string.IsNullOrWhiteSpace(id) || !System.Text.RegularExpressions.Regex.IsMatch(id, @"^[A-Za-z0-9.-]{1,64}$"))
            {
                return BadRequest();
            }

            if (string.IsNullOrWhiteSpace(system) || string.IsNullOrWhiteSpace(code))
            {
                return BadRequest();
            }

            var validationResult = _terminologyService.ValidateValueSetCode(id, system, code, display);

            if (!validationResult.ValueSetFound)
            {
                return NotFound();
            }

            var parameters = new Parameters
            {
                { "result", new FhirBoolean(validationResult.IsValid) },
                { "system", new FhirUri(validationResult.System) },
                { "code", new Code(validationResult.Code) }
            };

            if (!string.IsNullOrWhiteSpace(validationResult.ExpectedDisplay))
            {
                parameters.Add("display", new FhirString(validationResult.ExpectedDisplay));
            }

            if (!string.IsNullOrWhiteSpace(validationResult.Message))
            {
                parameters.Add("message", new FhirString(validationResult.Message));
            }

            return new FhirResult(parameters);

        }
    }
}