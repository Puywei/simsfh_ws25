
using Newtonsoft.Json;
using sims_web_app.Components.Identity.Contracts;

namespace sims_web_app.Components.Identity.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;

    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    public async Task<AuthResponseUserLogon?> Login(AuthRequest request)
    {
        var result = await _httpClient.PostAsJsonAsync("auth/login", request);
        if (result.IsSuccessStatusCode)
        {
            var content = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AuthResponseUserLogon>(content);
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
}