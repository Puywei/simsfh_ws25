using sims_web_app.Data.Model;

namespace sims_web_app.Services;
using RestSharp;

public static class BackendApiHandler
{
    private static readonly string baseUrl = "http://localhost:5001/api/v1/"; // Dein Endpoint

    public static async Task<Incident?> CreateIncident(Incident incident)
    {
        var client = new RestClient(baseUrl + "incidents");
        var request = new RestRequest("", Method.Post);
        request.AddJsonBody(incident);

        var response = await client.ExecuteAsync<Incident>(request);

        if (response.IsSuccessful)
        {
            return response.Data;
        }
        else
        {
            throw new Exception($"Error creating incident: {response.StatusCode} - {response.Content}");
        }
    }
}