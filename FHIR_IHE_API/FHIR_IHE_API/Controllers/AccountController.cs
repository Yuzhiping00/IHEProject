
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FHIR_IHE_API.Data;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using LoginRequest = FHIR_IHE_API.Models.LoginRequest;

namespace FHIR_IHE_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        private readonly IConfiguration _configuration;

        // Injecting database in constructor
        public AccountController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        //POST: api/account/login
        [HttpPost("login")]
        public Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
          // validate credentials (this should be done via a database query)

          if (loginRequest is not {Email: "test@example.com", Password: "password"})
              return Task.FromResult<IActionResult>(Unauthorized("Invalid email or password. Please try again.")); // 401

          var token = GenerateJwtToken(loginRequest.Email);

          var user = new
          {
              loginRequest.Email
          };

          return Task.FromResult<IActionResult>(Ok(new {token, user}));

        }

        private string GenerateJwtToken(string email)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name,email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);

        }


    }
}
