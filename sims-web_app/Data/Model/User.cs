using sims_web_app.Data.Model.Enum;

namespace sims_web_app.Data.Model;


public class User
{
    public string User_ID { get; set; }
    public string Name { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Password { get; set; }
    public string Role { get; set; }
    public string Email { get; set; }
}