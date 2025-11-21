using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using sims_web_app.Data.Model;
using RestSharp;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.IdentityModel.Tokens;

namespace sims_web_app.Services;

public class BackendApiHandler
{
    private static readonly string baseUrl = Environment.GetEnvironmentVariable("BackendApiBaseUrl");
    private readonly ProtectedBrowserStorage _protectedLocalStorage;
    public BackendApiHandler(ProtectedLocalStorage protectedLocalStorage)
    {
        _protectedLocalStorage = protectedLocalStorage;
    }
    // ****************** Incident *********************
    
    private static (RestRequest, RestClient) RestRequestHelper(string subUrl, Method method)
    {
        RestClient client = new RestClient(baseUrl);
        RestRequest request = new RestRequest(subUrl, method);
        return (request, client);
    }
    
    private async Task<bool> GetIsUserValid()
    {
        /*
        string GenerateToken(string email)
        {
            var key = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_KEY"));
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    // new Claim(ClaimTypes.NameIdentifier, user.Uid.ToString()),
                    new Claim(ClaimTypes.Name, email),
                    // new Claim(ClaimTypes.Role, user.Role.RoleName)
                }),
                Expires = DateTime.UtcNow.AddSeconds(5),
                Issuer = "sims-api",
                Audience = "sims-api",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            
            return tokenHandler.WriteToken(token);
        }

        string token = GenerateToken("admin@admin.com");
        */
        
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
                ValidIssuer = "sims-api",
                ValidateAudience = true,
                ValidAudience = "sims-api",
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
    
    public async Task<Incident?> CreateIncident(Incident incident)
    {
        if (await GetIsUserValid())
        {
            (RestRequest request,RestClient client) = RestRequestHelper("/Incidents", Method.Post);
            incident.Comments = new List<IncidentComment>();
            request.AddJsonBody(incident);
            RestResponse<Incident> response = await client.ExecuteAsync<Incident>(request);
        
            return response.Data;
        }
        return  null;
    }

    public async Task<List<Incident>?> GetAllIncidents()
    {
        if (await GetIsUserValid())
        {
            (RestRequest request,RestClient client) = RestRequestHelper("/Incidents", Method.Get);
            RestResponse<List<Incident>> response = await client.ExecuteAsync<List<Incident>>(request);
        
            return response.Data;
        }
        return null;
    }

    public async Task<Incident?> GetIncidentById(string id)
    {
        if (await GetIsUserValid())
        {
            RestClient client = new RestClient(baseUrl + $"/Incidents/{id}");
            RestRequest request = new RestRequest("");
            RestResponse<Incident> response = await client.ExecuteAsync<Incident>(request);
        
            return response.Data;
        }
        return null;
    }
    
    public async Task<bool> DeleteIncident(string id)
    {
        if (await GetIsUserValid())
        {
            (RestRequest request,RestClient client) = RestRequestHelper($"/Incidents/{id}", Method.Delete);
            RestResponse response = await client.ExecuteAsync(request);

            return response.StatusCode == HttpStatusCode.NoContent
                   || response.StatusCode == HttpStatusCode.OK;
        }
        return false;
    }

    public async Task<bool> UpdateIncident(Incident incident, string incidentId)
    {
        if (await GetIsUserValid())
        {
            (RestRequest request,RestClient client) = RestRequestHelper($"/Incidents/{incidentId}", Method.Put);
            request.AddJsonBody(incident);
            RestResponse response = await client.ExecuteAsync(request);

            return response.StatusCode == HttpStatusCode.NoContent
                   || response.StatusCode == HttpStatusCode.OK;
        }

        return false;
    }
    
    // ******************  Customer  *********************
    
    public async Task<bool> CreateCustomer(Customer customer)
    {
        if (await GetIsUserValid())
        {
            (RestRequest request,RestClient client) = RestRequestHelper("/Customers", Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddJsonBody(customer);
            RestResponse<Customer> response = await client.ExecuteAsync<Customer>(request);
        
            return (response.StatusCode == HttpStatusCode.Created);
        }
        
        return false;

    }
    
    public async Task<bool> DeleteCustomer(string customerId)
    {
        if (await GetIsUserValid())
        {
            (RestRequest request,RestClient client) = RestRequestHelper($"/Customers/{customerId}", Method.Delete);

            RestResponse response = await client.ExecuteAsync(request);
        
            if (response.StatusCode == HttpStatusCode.NoContent)
                return true;
        }
        return false;
    }
    
    public async Task<List<Customer>> GetAllCustomer()
    {
        if (await GetIsUserValid())
        {
            (RestRequest request,RestClient client) = RestRequestHelper("/Customers", Method.Get);
            RestResponse<List<Customer>> response = await client.ExecuteAsync<List<Customer>>(request);

            if (response.Data != null)
            {
                return response.Data.ToList();
            }
            else
            {
                return  new List<Customer>();
            }
        }
        return  new List<Customer>();
    }
    
    public async Task<Customer> GetCustomerById(string id)
    {
        if (await GetIsUserValid())
        {
            RestClient client = new RestClient(baseUrl);
            RestRequest request = new RestRequest($"Customers/{id}");
        
            var response = await client.ExecuteAsync<Customer>(request);
            return response.Data!;
        }
       return new Customer();

    }
    
    public async Task<bool> UpdateCustomer(Customer customer, string customerId)
    {
        if (await GetIsUserValid())
        {
            (RestRequest request,RestClient client) = RestRequestHelper($"/Customers/{customerId}", Method.Put);
            request.AddHeader("Content-Type", "application/json");
            request.AddJsonBody(customer);
            RestResponse<Customer> response = await client.ExecuteAsync<Customer>(request);
        
            return (response.StatusCode == HttpStatusCode.OK);
        }
        return false;
    }
    
}