using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace sims.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        [HttpGet("me")]
        [Authorize] 
        public IActionResult GetMe()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = User.Identity?.Name;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            return Ok(new { userId, email, role });
        }
    }
}