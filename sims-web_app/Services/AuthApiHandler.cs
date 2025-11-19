using Microsoft.AspNetCore.Components;
using RestSharp;
using RestSharp.Authenticators;
using sims_web_app.Components.Identity.Contracts;
using sims_web_app.Data.Model;

namespace sims_web_app.Services;

public class AuthApiHandler
{
    private static readonly string baseUrl = Environment.GetEnvironmentVariable("AuthApiBaseUrl");
    private readonly TokenProvider _tokenProvider;

    public AuthApiHandler(TokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider;
    }

    // ****************** Incident *********************
    
    private static (RestRequest, RestClient) RestRequestHelper(string subUrl, Method method)
    {
        RestClient client = new RestClient(baseUrl);
        RestRequest request = new RestRequest(subUrl, method);
        return (request, client);
    }
    
    public async Task<List<User>> GetAllUsers()
    {
        
        (RestRequest request,RestClient client) = RestRequestHelper("/Users/getAll", Method.Get);
        request.Authenticator = new JwtAuthenticator(_tokenProvider.AccessToken);
        RestResponse<List<User>> response = await client.ExecuteAsync<List<User>>(request);
        return response.Data;
    }
    
    public async Task<bool> CreateUser(User user)
    {
        (RestRequest request,RestClient client) = RestRequestHelper("/Users/create", Method.Post);
        request.Authenticator = new JwtAuthenticator(_tokenProvider.AccessToken);
        request.AddJsonBody(user);
        RestResponse response = await client.ExecuteAsync(request);

        return (response.IsSuccessful);
    }
    
    public async Task<bool> ModifyUser(AuthResponseUser user, int userId )
    {
        (RestRequest request,RestClient client) = RestRequestHelper($"/Users/modify?uid={userId}", Method.Put);
        request.Authenticator = new JwtAuthenticator(_tokenProvider.AccessToken);
        request.AddJsonBody(user);
        RestResponse response = await client.ExecuteAsync(request);

        return (response.IsSuccessful);
    }
    
    public async Task<bool> DeleteUser(int userId)
    {
        (RestRequest request,RestClient client) = RestRequestHelper($"/Users/delete?uid={userId}", Method.Delete);
        request.Authenticator = new JwtAuthenticator(_tokenProvider.AccessToken);
        RestResponse response = await client.ExecuteAsync(request);

        return (response.IsSuccessful);
    }
}