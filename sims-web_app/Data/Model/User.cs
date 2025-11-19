using sims_web_app.Components.Identity.Contracts;
using sims_web_app.Data.Model.Enum;

namespace sims_web_app.Data.Model;


public class User : AuthResponseUser
{
    public int Id { get; set; }
}