using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace sims_web_app.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private AuthenticationState authenticationState;

        public CustomAuthStateProvider(IAuthService service)
        {
            authenticationState = new AuthenticationState(service.CurrentUser);

            service.UserChanged += (newUser) =>
            {
                string? role = newUser.Claims.Where(q => q.Type == "role").Select(q => q.Value)
                    .FirstOrDefault();

                if (!String.IsNullOrEmpty(role))
                {

                    ((ClaimsIdentity) newUser.Identity).AddClaim(new Claim(ClaimTypes.Role, role)); 
                }
                
                authenticationState = new AuthenticationState(newUser);
                
                NotifyAuthenticationStateChanged(
                    Task.FromResult(new AuthenticationState(newUser)));
            };
        }
        public override Task<AuthenticationState> GetAuthenticationStateAsync() =>
            Task.FromResult(authenticationState);
    }
}
