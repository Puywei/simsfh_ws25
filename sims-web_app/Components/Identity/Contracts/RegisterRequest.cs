namespace sims_web_app.Components.Identity.Contracts;

public class RegisterRequest
{
    public string? FullName { get; set; }
    public string? Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string[]? Roles { get; set; }
}