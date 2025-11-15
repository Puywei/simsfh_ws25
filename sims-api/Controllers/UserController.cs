using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sims.Data;
using sims.Models;
using System.Security.Claims;

namespace sims.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserDbContext _db;

        public UsersController(UserDbContext db)
        {
            _db = db;
        }

        //  Create user endpoint
        [HttpPost("create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
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

        // Modify user endpoint
        [HttpPut("modify")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Modify(int uid, [FromBody] ModifyUserRequest request)
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

        //  Delete user endpoint
        [HttpDelete("delete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int uid)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Uid == uid);
            if (user == null)
                return NotFound(new { message = "User not found." });

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();

            return Ok(new { message = "User successfully deleted.", uid = uid });
        }

        //  Get own user endpoint
        [HttpGet("getCurrent")]
                 [Authorize]
                 public IActionResult GetMe()
                 {
                     var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                     var email = User.Identity?.Name;
                     var role = User.FindFirst(ClaimTypes.Role)?.Value;
         
                     return Ok(new { userId, email, role });
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

    public class ModifyUserRequest
    {
        public string? Firstname { get; set; }
        public string? Lastname { get; set; }
        public string? Email { get; set; }
        public int? RoleId { get; set; }
        public string? Password { get; set; }
    }
}
