namespace sims_web_app.Components.Identity.Contracts;

public class AuthResponseUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public int RoleId { get; set; }
    public string Password { get; set; }
    
    public string token { get; set; }
    // public string role { get; set; }
}