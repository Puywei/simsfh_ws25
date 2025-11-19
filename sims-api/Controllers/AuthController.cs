using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using sims.Data;
using sims.Models;
using sims.Services;

namespace sims.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserDbContext _db;
        private readonly IConfiguration _config;
        private readonly IEventLogger _eventLogger;

        public AuthController(UserDbContext db, IConfiguration config, IEventLogger eventLogger)
        {
            _db = db;
            _config = config;
            _eventLogger = eventLogger;
        }

        //  Login endpoint
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _db.Users.Include(u => u.Role)
                                      .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Unauthorized("Invalid credentials.");

            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Uid.ToString()),
                    new Claim(ClaimTypes.Name, user.Email),
                    new Claim(ClaimTypes.Role, user.Role.RoleName)
                }),
                Expires = DateTime.UtcNow.AddHours(4),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Issuer"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            
            await _eventLogger.LogEventAsync(
                $"User logged in: '{user.Email}' (Role='{user.Role.RoleName}')",
                severity: 1
            );
            
            return Ok(new
            {
                message = "Login successful",
                token = tokenHandler.WriteToken(token),
                uid = user.Uid,
                firstname = user.Firstname,
                lastname = user.Lastname,
                role = user.Role.RoleName
            });
        }
        
        // Logout
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
            
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return BadRequest(new { message = "No token found in request." });

            var actingUser = UserContextHelper.GetActingUserInfo(User);
            
            var token = authHeader.Substring("Bearer ".Length).Trim();

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var expiry = jwtToken.ValidTo;

            _db.BlacklistedTokens.Add(new BlacklistedToken
            {
                Token = token,
                Expiry = expiry
            });
            await _db.SaveChangesAsync();
            
            await _eventLogger.LogEventAsync(
                $"User: '{actingUser.Email}' with Role '{actingUser.Role}' logged out.",
                severity: 1
            );

            return Ok(new { message = "Logout successful. Token invalidated." });
            
        }
    }

    // DTOs
    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
