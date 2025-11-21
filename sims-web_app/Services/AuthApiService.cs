
using System.Security.Claims;
using Newtonsoft.Json;
using sims_web_app.Components.Identity.Contracts;
using sims_web_app.Data.Model;

namespace sims_web_app.Services;
public class AuthApiService
{
    private readonly HttpClient _httpClient;


    public AuthApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
  /*  public async Task<ClaimsPrincipal> Login(string email, string password)
    {
        AuthRequest authRequest = new()
        {
            email = email,
            Password = password
        };
        
        var result = await _httpClient.PostAsJsonAsync("auth/login", authRequest);
        if (result.IsSuccessStatusCode)
        {
            var content = await result.Content.ReadAsStringAsync();
            var jsonContent = JsonConvert.DeserializeObject<AuthResponseUser>(content);
             
             var claims = new List<Claim>
             {
                 new Claim(ClaimTypes.GivenName, jsonContent.FirstName),
                 new Claim(ClaimTypes.Name, jsonContent.LastName),
                 new Claim(ClaimTypes.Email, jsonContent.Email),
             };
             var identity = new ClaimsIdentity(claims, "jwt");
             var principal = new ClaimsPrincipal(identity);
             return principal;
        }
        else
        {
            return null;
        }
    }
    public async Task<bool> Register(RegisterRequest request)
    {
        var result = await _httpClient.PostAsJsonAsync("auth/register", request);
        return result.IsSuccessStatusCode;
    }
    */
}