
using FHIR_IHE_API.Identity;
using FHIR_IHE_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LoginRequest = FHIR_IHE_API.Models.LoginRequest;

namespace FHIR_IHE_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly IConfiguration _configuration;

        // Injecting database in constructor
        public AccountController(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        //POST: api/account/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            // validate credentials (this should be done via a database query)

            if (string.IsNullOrWhiteSpace(loginRequest.Email) ||
                string.IsNullOrWhiteSpace(loginRequest.Password))
            {
                return Unauthorized(
                    "Invalid email or password.");
            }

            // ----------------------------------------
            // Find user
            // ----------------------------------------

            var user = await _userManager.FindByEmailAsync(loginRequest.Email);

            if (user == null)
            {
                return Unauthorized("Invalid email or password");
            }

            // ----------------------------------------
            // Verify password
            // ----------------------------------------

            var passwordValid = await _userManager.CheckPasswordAsync(user, loginRequest.Password);

            if (!passwordValid)
            {
                return Unauthorized("Invalid email or password");
            }

            // ----------------------------------------
            // Get user roles
            // ----------------------------------------

            var roles = await _userManager.GetRolesAsync(user);

            var role = roles.FirstOrDefault();

            if (role == null)
            {
                return Unauthorized("User has no assigned role");
            }

            // ----------------------------------------
            // Generate JWT
            // ----------------------------------------

            var token = GenerateJwtToken(user, role);

            // ----------------------------------------
            // Return user information
            // ----------------------------------------

            var responseUser = new
            {
                id = user.Id,
                email = user.Email,
                role,
                patientId = user.PatientId
            };

            return Ok(new { token, user = responseUser });

        }

        private string GenerateJwtToken(ApplicationUser user, string role)
        {

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id),

                new(ClaimTypes.NameIdentifier, user.Id),

                new (ClaimTypes.Name, user.Email ?? ""),

                new (ClaimTypes.Email, user.Email ?? ""),

                new (ClaimTypes.Role, role)
            };

            if (user.PatientId.HasValue)
            {
                claims.Add(new Claim("patientId", user.PatientId.Value.ToString()));
            }

            var secret = _configuration["Jwt:Secret"];

            if (string.IsNullOrWhiteSpace(secret))
            {
                throw new InvalidOperationException("JWT secret is not configured");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [Authorize(Policy = "PatientOnly")]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return BadRequest("Current password and new password are required");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return Unauthorized();
            }

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

            if (!result.Succeeded)
            {
                return BadRequest(new
                {
                    errors = result.Errors.Select(e => e.Description)
                });
            }

            return Ok(new { message = "Password changed successfully" });
        }

    }
}
