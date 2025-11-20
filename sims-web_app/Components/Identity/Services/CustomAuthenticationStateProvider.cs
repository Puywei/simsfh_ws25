using System.Security.Claims;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace sims_web_app.Components.Identity.Services;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ProtectedLocalStorage _sessionStorageService;
    private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

    public CustomAuthenticationStateProvider(ProtectedLocalStorage sessionStorageService)
    {
        _sessionStorageService = sessionStorageService;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        // Während Prerendering: direkt anonym zurückgeben
        if (!OperatingSystem.IsBrowser())
            return new AuthenticationState(_anonymous);

        var token = await _sessionStorageService.GetAsync<string>("token");
        if (string.IsNullOrEmpty(token.ToString()))
            return new AuthenticationState(_anonymous);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token.ToString());

        var identity = new ClaimsIdentity(jwt.Claims, "jwt");
        var user = new ClaimsPrincipal(identity);

        return new AuthenticationState(user);
    }


    public void AuthenticateUser(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        //test
        var claims = jwt.Claims
               .Select(c => c.Type switch
               {
                   "role" => new Claim(ClaimTypes.Role, c.Value),  
                   "unique_name" => new Claim(ClaimTypes.Name, c.Value),   
                   _ => c
               })
               .ToList();

        var identity = new ClaimsIdentity(
                                claims,
                                "jwt",
                             ClaimTypes.Name,  
                             ClaimTypes.Role   
                         );
        var user = new ClaimsPrincipal(identity);

        var state = new AuthenticationState(user);
        NotifyAuthenticationStateChanged(Task.FromResult(state));
    }
}