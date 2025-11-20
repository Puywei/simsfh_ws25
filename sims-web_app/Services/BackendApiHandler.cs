using System.Diagnostics;
using System.Net;
using sims_web_app.Data.Model;
using RestSharp;
using System.Net.Http; 

namespace sims_web_app.Services;

public static class BackendApiHandler
{
    private static readonly string baseUrl = Environment.GetEnvironmentVariable("BackendApiBaseUrl");

    // ****************** Incident *********************
    
    private static (RestRequest, RestClient) RestRequestHelper(string subUrl, Method method)
    {
        RestClient client = new RestClient(baseUrl);
        RestRequest request = new RestRequest(subUrl, method);
        return (request, client);
    }
    

    public static async Task<Incident> CreateIncident(Incident incident)
    {
        (RestRequest request,RestClient client) = RestRequestHelper("/Incidents", Method.Post);
        incident.Comments = new List<IncidentComment>();
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

    public static async Task<Incident?> GetIncidentById(string id)
    {
        RestClient client = new RestClient(baseUrl + $"/Incidents/{id}");
        RestRequest request = new RestRequest("");
        RestResponse<Incident> response = await client.ExecuteAsync<Incident>(request);
        
        return response.Data;
    }
    
    public static async Task<bool> DeleteIncident(string id)
    {
        (RestRequest request,RestClient client) = RestRequestHelper($"/Incidents/{id}", Method.Delete);
        RestResponse response = await client.ExecuteAsync(request);

        return response.StatusCode == HttpStatusCode.NoContent
        || response.StatusCode == HttpStatusCode.OK;
    }

    public static async Task<bool> UpdateIncident(Incident incident, string incidentId)
    {
        (RestRequest request,RestClient client) = RestRequestHelper($"/Incidents/{incidentId}", Method.Put);
        request.AddJsonBody(incident);
        RestResponse response = await client.ExecuteAsync(request);

        return response.StatusCode == HttpStatusCode.NoContent
       || response.StatusCode == HttpStatusCode.OK;
    }
    // ******************  Customer  *********************
    
    public static async Task<bool> CreateCustomer(Customer customer)
    {
        (RestRequest request,RestClient client) = RestRequestHelper("/Customers", Method.Post);
        request.AddHeader("Content-Type", "application/json");
        request.AddJsonBody(customer);
        RestResponse<Customer> response = await client.ExecuteAsync<Customer>(request);
        
        return (response.StatusCode == HttpStatusCode.Created);
    }
    
    public static async Task<bool> DeleteCustomer(string customerId)
    {
        (RestRequest request,RestClient client) = RestRequestHelper($"/Customers/{customerId}", Method.Delete);

        RestResponse response = await client.ExecuteAsync(request);
        
        if (response.StatusCode == HttpStatusCode.NoContent)
            return true;
        return false;
    }
    
    public static async Task<List<Customer>> GetAllCustomer()
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
    
    public static async Task<Customer> GetCustomerById(string id)
    {
       
        RestClient client = new RestClient(baseUrl);
        RestRequest request = new RestRequest($"Customers/{id}");
        
        var response = await client.ExecuteAsync<Customer>(request);
        
        return response.Data!;
    }
    
    public static async Task<bool> UpdateCustomer(Customer customer, string customerId)
    {
        (RestRequest request,RestClient client) = RestRequestHelper($"/Customers/{customerId}", Method.Put);
        request.AddHeader("Content-Type", "application/json");
        request.AddJsonBody(customer);
        RestResponse<Customer> response = await client.ExecuteAsync<Customer>(request);
        
        return (response.StatusCode == HttpStatusCode.OK);
    }
    
}