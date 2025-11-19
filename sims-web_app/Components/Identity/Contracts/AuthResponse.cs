namespace sims_web_app.Components.Identity.Contracts;

public class AuthResponse
{
    public string message { get; set; }
    public string Token { get; set; }
    public int uid { get; set; }
    public string firstname { get; set; }
    public string lastname { get; set; }
    public string role { get; set; }
}