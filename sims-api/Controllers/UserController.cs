using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using sims.Data;
using sims.Models;
using sims.Services;
using sims.Models.DTOs;
using sims.Helpers;

namespace sims.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserDbContext _db;
        private readonly IEventLogger _eventLogger;

        public UsersController(UserDbContext db, IEventLogger eventLogger)
        {
            _db = db;
            _eventLogger = eventLogger;
        }

        //  Create user endpoint
        [HttpPost("create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
        {
            
            var actingUser = UserContextHelper.GetActingUserInfo(User);
            
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            if (await _db.Users.AnyAsync(u => u.Email == request.Email))
                return BadRequest("Email already exists.");
            
            var roleId = request.RoleId ?? 2;

            var roleExists = await RoleHelper.CheckIfRoleExists(_db, roleId);
            if (!roleExists)
                return BadRequest(new { message = $"RoleId '{roleId}' does not exist." });


            var user = new User
            {
                Firstname = request.Firstname,
                Lastname = request.Lastname,
                Email = request.Email,
                RoleId = roleId,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            await _eventLogger.LogEventAsync(
                $"Acting User:'{actingUser.Email}' - User created: UserID:'{user.Uid}' '{user.Email}' (RoleId='{user.RoleId}')",
                severity: 2
            );
            

            return Ok(new { message = "User successfully created.", uid = user.Uid });
        }

        // Modify user endpoint
        [HttpPut("modify")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Modify(int uid, [FromBody] ModifyUserRequest request)
        {
            var actingUser = UserContextHelper.GetActingUserInfo(User);
            
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Uid == uid);
            if (user == null)
                return NotFound("User not found.");
            
            var changes = new List<string>();

            if (!string.IsNullOrWhiteSpace(request.Firstname) && request.Firstname != user.Firstname)
            {
                user.Firstname = request.Firstname;
                changes.Add($"Firstname → {request.Firstname}");
            }

            if (!string.IsNullOrWhiteSpace(request.Lastname) && request.Lastname != user.Lastname)
            {
                user.Lastname = request.Lastname;
                changes.Add($"Lastname → {request.Lastname}");
            }

            if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != user.Email)
            {
                if (await _db.Users.AnyAsync(u => u.Email == request.Email && u.Uid != uid))
                    return BadRequest("Email already exists.");
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                
                user.Email = request.Email;
                changes.Add($"Email → {request.Email}");
            }

            if (request.RoleId.HasValue && request.RoleId.Value != user.RoleId)
            {
                var roleId = request.RoleId ?? 2;
            
                var roleExists = await RoleHelper.CheckIfRoleExists(_db, roleId);
                if (!roleExists)
                    return BadRequest(new { message = $"RoleId '{roleId}' does not exist." });
                
                user.RoleId = request.RoleId.Value;
                changes.Add($"RoleId → {request.RoleId}");
            }

            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                changes.Add($"Password has been changed!");
            }

            await _db.SaveChangesAsync();
            
            if (changes.Any())
            {
                var changeSummary = string.Join(", ", changes);
                await _eventLogger.LogEventAsync(
                    $"Acting User:{actingUser.Email} - User {user.Email} (UserID = {user.Uid}) modified. Changes: {changeSummary}",
                    severity: 2
                );
            }
                
            return Ok(new { message = "User successfully updated.", uid = user.Uid });
        }

        //  Delete user endpoint
        [HttpDelete("delete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int uid)
        {
            var actingUser = UserContextHelper.GetActingUserInfo(User);
            
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Uid == uid);
            if (user == null)
                return NotFound(new { message = "User not found." });

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
            
            
            await _eventLogger.LogEventAsync(
                $"Acting User:{actingUser.Email} - User deleted: UserID:{user.Uid} '{user.Email}'",
                severity: 2
            );

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
        //  Get all users endpoint
        [HttpGet("getAllUsers")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _db.Users
                .Include(u => u.Role)
                .Select(u => new
                {
                    uid = u.Uid,
                    firstname = u.Firstname,
                    lastname = u.Lastname,
                    email = u.Email,
                    role = u.Role.RoleName
                })
                .ToListAsync();

            return Ok(users);
        }
        
        [HttpGet("getAllRoles")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _db.Roles
                .Select(r => new
                {
                    Roleid = r.RoleId,
                    RoleName = r.RoleName
                })
                .ToListAsync();

            return Ok(roles);
        }
    }
    
}
