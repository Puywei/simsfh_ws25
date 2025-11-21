using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using RestSharp;
using sims_web_app.Data.Model;

namespace sims_web_app.Services;

public class LogApiHandler
{
    private static readonly string baseUrl = Environment.GetEnvironmentVariable("LogApiBaseUrl");
    private readonly ProtectedBrowserStorage _protectedLocalStorage;

    public LogApiHandler(ProtectedLocalStorage protectedLocalStorage)
    {
        _protectedLocalStorage = protectedLocalStorage;
    }
    

    private static (RestRequest, RestClient) RestRequestHelper(string subUrl, Method method)
    {
        RestClient client = new RestClient(baseUrl);
        RestRequest request = new RestRequest(subUrl, method);
        return (request, client);
    }
    private async Task<bool> GetIsUserValid()
    {
 
        var result = await _protectedLocalStorage.GetAsync<string>("token");
        if (string.IsNullOrWhiteSpace(result.Value))
            return false;

        var token = result.Value;
        var handler = new JwtSecurityTokenHandler();

        try
        {
            var validationParams = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER"),
                ValidateAudience = true,
                ValidAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_KEY"))),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            ClaimsPrincipal principal = handler.ValidateToken(token, validationParams, out _);
            
            return principal.Identity.IsAuthenticated;
        }
        catch (Exception exception)
        {
            return false; 
        }
    }
    
    public async Task<List<LogEntry>> GetAllLogs()
    {
        if (await GetIsUserValid())
        {
            (RestRequest request,RestClient client) = RestRequestHelper("/Redis/logs", Method.Get);
            RestResponse<List<LogEntry>> response = await client.ExecuteAsync<List<LogEntry>>(request);

            if (response.Content != null)
            {
                return JsonConvert.DeserializeObject<List<LogEntry>>(response.Content);
            }
            else
            {
                return  new List<LogEntry>();
            }
        }
        return  new List<LogEntry>();
    }
}