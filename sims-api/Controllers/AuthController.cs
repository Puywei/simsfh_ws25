using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using sims.Data;
using sims.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace sims.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserDbContext _db;
        private readonly IConfiguration _config;

        public AuthController(UserDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        // Create a new user
        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> Register([FromBody] CreateUserRequest request)
        {
            if (await _db.Users.AnyAsync(u => u.Email == request.Email))
                return BadRequest("Email already exists.");

            var user = new User
            {
                Firstname = request.Firstname,
                Lastname = request.Lastname,
                Email = request.Email,
                RoleId = request.RoleId,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return Ok(new { message = "User successfully created.", uid = user.Uid });
        }

        // Login and here is also JWT Token created
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _db.Users.Include(u => u.Role)
                                      .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Unauthorized("Invalid credentials.");

            
            var tokenHandler = new JwtSecurityTokenHandler();
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

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwt = tokenHandler.WriteToken(token);

            return Ok(new
            {
                message = "Login successful",
                token = jwt,
                uid = user.Uid,
                firstname = user.Firstname,
                lastname = user.Lastname,
                role = user.Role.RoleName
            });
        }
        
        // Modify User API
        [HttpPut("modify")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ModifyUser(int uid, [FromBody] ModifyUserRequest request)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Uid == uid);
            if (user == null)
                return NotFound("User not found.");

            
            if (!string.IsNullOrEmpty(request.Firstname))
                user.Firstname = request.Firstname;

            if (!string.IsNullOrEmpty(request.Lastname))
                user.Lastname = request.Lastname;

            if (!string.IsNullOrEmpty(request.Email))
            {
             
                if (await _db.Users.AnyAsync(u => u.Email == request.Email && u.Uid != uid))
                    return BadRequest("Email already exists.");
                user.Email = request.Email;
            }

            if (request.RoleId.HasValue)
                user.RoleId = request.RoleId.Value;

            if (!string.IsNullOrEmpty(request.Password))
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            await _db.SaveChangesAsync();

            return Ok(new { message = "User successfully updated.", uid = user.Uid });
        }
        
        //Delete User Endpoint
        [HttpDelete("delete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int uid)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Uid == uid);
            if (user == null)
                return NotFound(new { message = "User not found."});
            
            _db.Users.Remove(user);

            await _db.SaveChangesAsync();

            return Ok(new { message = "User successfully updated.", uid = user.Uid });
        }
        
        
        
    }

    // DTOs
    public class CreateUserRequest
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; }
        public int RoleId { get; set; }
        public string Password { get; set; }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class ModifyUserRequest
    {
        public string? Firstname { get; set; }
        public string? Lastname { get; set; }
        public string? Email { get; set; }
        public int? RoleId { get; set; }
        public string? Password { get; set; }
    }
    
    
    
}
