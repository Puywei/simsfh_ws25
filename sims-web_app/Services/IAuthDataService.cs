using System.Security.Claims;
using sims_web_app.Data.Model;

namespace sims_web_app.Services
{
    public interface IAuthDataService
    {
        ServiceResponse<ClaimsPrincipal> Login(string email, string password);
    }
}
