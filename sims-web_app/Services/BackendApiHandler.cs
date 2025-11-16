using sims_web_app.Data.Model;

namespace sims_web_app.Services;
using RestSharp;

public static class BackendApiHandler
{
    private static readonly string baseUrl = Environment.GetEnvironmentVariable("BackendApiBaseUrl");
    
    private static (RestRequest, RestClient) RestRequestHelper(string subUrl, Method method)
    {
        RestClient client = new RestClient(baseUrl + subUrl);
        RestRequest request = new RestRequest("", method);
        return (request, client);
    }
    
    public static async Task<Incident?> CreateIncident(Incident incident)
    {
        (RestRequest request,RestClient client) = RestRequestHelper("/Incidents", Method.Post);
        request.AddJsonBody(incident);
        RestResponse<Incident> response = await client.ExecuteAsync<Incident>(request);
        
        return response.Data;
    }

    public static async Task<List<Incident>> GetAllIncidents()
    {
        (RestRequest request,RestClient client) = RestRequestHelper("/Incidents", Method.Get);
        RestResponse<List<Incident>> response = await client.ExecuteAsync<List<Incident>>(request);
        
        return response.Data;
    }

    public static async Task<Incident?> GetIncidentById(int id)
    {
        RestClient client = new RestClient(baseUrl + $"/Incidents/{id}");
        RestRequest request = new RestRequest("");
        RestResponse<Incident> response = await client.ExecuteAsync<Incident>(request);
        
        return response.Data;
    }
    
    public static async Task<RestResponse> DeleteIncident(int id)
    {
        (RestRequest request,RestClient client) = RestRequestHelper($"/Incidents/{id}", Method.Delete);
        RestResponse response = await client.ExecuteAsync(request);
        
        return response;
    }

    public static async Task<RestResponse> UpdateIncident(Incident incident, string incidentId)
    {
        (RestRequest request,RestClient client) = RestRequestHelper($"/Incidents/{incidentId}", Method.Put);
        request.AddJsonBody(incident);
        RestResponse response = await client.ExecuteAsync(request);
        
        return response;
    }
    
}