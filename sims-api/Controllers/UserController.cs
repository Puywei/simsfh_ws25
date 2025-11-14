using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UserApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    // Public endpoint -> Does NOT need Token from Keycloak... Perhaps for testing? Read-only access? ...
    [HttpGet("public")]
    public IActionResult PublicEndpoint()
    {
        return Ok("This is a public endpoint.");
    }

    // Secure endpoint (requires Keycloak JWT)
    [Authorize]
    [HttpGet("secure")]
    public IActionResult SecureEndpoint()
    {
        return Ok("This is a secure endpoint, only accessible with a valid Keycloak token.");
    }
}