using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using RestSharp;

namespace sims_web_app.Services;

public class LogApiHandler
{
    private static readonly string baseUrl = Environment.GetEnvironmentVariable("AuthApiBaseUrl");
    private readonly ProtectedBrowserStorage _protectedLocalStorage;

    public LogApiHandler(ProtectedLocalStorage protectedLocalStorage)
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
}