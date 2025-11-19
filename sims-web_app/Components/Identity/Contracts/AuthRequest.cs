namespace sims_web_app.Components.Identity.Contracts;

public class AuthRequest
{
    public string? email { get; set; } = string.Empty;
    public string? Password { get; set; } = string.Empty;
}