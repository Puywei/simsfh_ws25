using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using sims.Data;
using sims.Helpers;
using sims.Models.DTOs;
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
                return Unauthorized(new { message = "Invalid credentials." });


            var token = JwtTokenService.GenerateToken(user, _config);

            
            await _eventLogger.LogEventAsync(
                $"User logged in: '{user.Email}' (Role='{user.Role.RoleName}')",
                severity: 1
            );
            
            return Ok(new
            {
                message = "Login successful",
                token,
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

            await TokenBlacklistService.BlacklistToken(_db, token, expiry);

            
            await _eventLogger.LogEventAsync(
                $"User: '{actingUser.Email}' with Role '{actingUser.Role}' logged out.",
                severity: 1
            );

            return Ok(new { message = "Logout successful. Token invalidated." });
            
        }
    }
}
