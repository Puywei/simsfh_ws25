using System.Security.Claims;
using sims_web_app.Components.Identity.Contracts;
using sims_web_app.Data.Model;

namespace sims_web_app.Services
{
    /// <summary>
    /// This is an example of a data service that would be used to authenticate a user.
    /// </summary>
    public class AuthDataService : IAuthDataService
    {
        private readonly HttpClient _httpClient;
        
        public AuthDataService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        
        public ServiceResponse<ClaimsPrincipal> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email))
            {
                return new ServiceResponse<ClaimsPrincipal>("Email is required");
            }

            if (string.IsNullOrEmpty(password))
            {
                return new ServiceResponse<ClaimsPrincipal>("Password is required");
            }

            AuthRequest authRequest = new AuthRequest()
            {
                email = email,
                Password = password
            };

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, email.StartsWith("admin") ? "Admin" : "Super")
            };

            var identity = new ClaimsIdentity(claims, "jwt");
            var principal = new ClaimsPrincipal(identity);

            return new ServiceResponse<ClaimsPrincipal>("Login successful", true, principal);
        }
    }
}
