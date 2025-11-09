namespace sims_web_app.Components.Models.Entities;

public class UserAccount
{
    public int Id { get; internal set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string Role { get; set; }
    public string Email { get; set; }
}