using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using sims_web_app.Components.Identity.Contracts;
using sims_web_app.Data.Model;

namespace sims_web_app.Services;

public class AuthApiHandler
{
    private static readonly string baseUrl = Environment.GetEnvironmentVariable("AuthApiBaseUrl");
    private readonly ProtectedBrowserStorage _protectedLocalStorage;

    public AuthApiHandler(ProtectedLocalStorage protectedLocalStorage)
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

    private async Task<string> GetAccessToken()
    {
        ProtectedBrowserStorageResult<string> jsonToken = await _protectedLocalStorage.GetAsync<string>("token");

        if (jsonToken.Value == null)
            return "";

        return jsonToken.Value;
    }

    public async Task<List<User>> GetAllUsers()
    {
        string authToken = await GetAccessToken();

        if (String.IsNullOrEmpty(authToken))
        {
            return null;
        }
        
        (RestRequest request, RestClient client) = RestRequestHelper("/Users/getAllUsers", Method.Get);
        request.Authenticator = new JwtAuthenticator(authToken);
        RestResponse<List<User>> response = await client.ExecuteAsync<List<User>>(request);

        foreach (User user in response.Data)
            user.FullName = user.FirstName + " " + user.LastName;

        return response.Data;
    }

    public async Task<List<UserRole>> GetAllRoles()
    {
        string authToken = await GetAccessToken();

        if (String.IsNullOrEmpty(authToken))
        {
            return null;
        }
        (RestRequest request, RestClient client) = RestRequestHelper("/Users/getAllRoles", Method.Get);
        request.Authenticator = new JwtAuthenticator(authToken);
        RestResponse<List<UserRole>> response = await client.ExecuteAsync<List<UserRole>>(request);
        return response.Data;
    }


    public async Task<bool> CreateUser(User user)
    {
        string authToken = await GetAccessToken();

        if (String.IsNullOrEmpty(authToken))
        {
            return false;
        }
        
        (RestRequest request, RestClient client) = RestRequestHelper("/Users/create", Method.Post);
        request.Authenticator = new JwtAuthenticator(authToken);
        request.AddJsonBody(user);
        RestResponse response = await client.ExecuteAsync(request);

        return (response.IsSuccessful);
    }

    public async Task<bool> ModifyUser(AuthResponseUser user, int userId)
    {
        string authToken = await GetAccessToken();

        if (String.IsNullOrEmpty(authToken))
        {
            return false;
        }
        (RestRequest request, RestClient client) = RestRequestHelper($"/Users/modify?uid={userId}", Method.Put);
        request.Authenticator = new JwtAuthenticator(authToken);
        request.AddJsonBody(user);
        RestResponse response = await client.ExecuteAsync(request);

        return (response.IsSuccessful);
    }

    public async Task<bool> DeleteUser(int userId)
    {
        string authToken = await GetAccessToken();

        if (String.IsNullOrEmpty(authToken))
        {
            return false;
        }
        (RestRequest request, RestClient client) = RestRequestHelper($"/Users/delete?uid={userId}", Method.Delete);
        request.Authenticator = new JwtAuthenticator(authToken);
        RestResponse response = await client.ExecuteAsync(request);

        return (response.IsSuccessful);
    }

    public async Task<bool> Login(string email, string password)
    {
        AuthRequest authRequest = new AuthRequest()
        {
            email = email,
            Password = password
        };
            
        RestClient restClient = new RestClient(baseUrl);
        RestRequest request = new RestRequest("auth/login", Method.Post);
        request.AddJsonBody(authRequest);
        
        RestResponse response = await restClient.ExecuteAsync(request);
        
        if (response.IsSuccessStatusCode)
        {
            var jsonContent = JsonConvert.DeserializeObject<AuthResponseUser>(response.Content);

            JwtSecurityToken jwtSecurityToken = new JwtSecurityTokenHandler().ReadJwtToken(jsonContent.token);
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(jwtSecurityToken.Claims, "jwt"));

            if (!claimsPrincipal.Identity.IsAuthenticated)
                return false;
            
            string? name = claimsPrincipal.Claims.Where(q => q.Type == "unique_name").Select(q => q.Value)
                .FirstOrDefault();
            
            string? role = claimsPrincipal.Claims.Where(q => q.Type == "role").Select(q => q.Value)
                .FirstOrDefault();

            if (String.IsNullOrEmpty(name) || String.IsNullOrEmpty(role))
                return false;
            
            await _protectedLocalStorage.SetAsync("token", jsonContent.token);

            return true;
            
        }
        else
        {
            return false;
        }
    }

    public async Task<bool> Logout()
    {
        try
        {
            var jsonToken = await _protectedLocalStorage.GetAsync<string>("token");
            
            RestClient restClient = new RestClient(baseUrl);
            RestRequest request = new RestRequest("auth/logout", Method.Post);
            request.Authenticator = new JwtAuthenticator(jsonToken.Value);
            RestResponse response = await restClient.ExecuteAsync(request);
            
            
            
            
            await _protectedLocalStorage.DeleteAsync("token");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
        return true;
    }
}