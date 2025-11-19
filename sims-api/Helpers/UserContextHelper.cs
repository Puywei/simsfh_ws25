using System.Security.Claims;

public class ActingUserInfo
{
    public string UserId { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
}

public static class UserContextHelper
{
    public static ActingUserInfo GetActingUserInfo(ClaimsPrincipal user)
    {
        return new ActingUserInfo
        {
            UserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown-id",
            Email = user.Identity?.Name ?? "unknown-email",
            Role = user.FindFirst(ClaimTypes.Role)?.Value ?? "unknown-role"
        };
    }
}