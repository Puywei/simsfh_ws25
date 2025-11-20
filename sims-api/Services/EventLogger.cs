using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace sims.Services
{
    public class EventLogger : IEventLogger
    {
        private readonly HttpClient _httpClient;

        public EventLogger(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("EventLogger");
        }

        public async Task LogEventAsync(string message,int severity = 1)
        {
            var eventPayload = new
            {
                logId = 5, 
                timestamp = DateTime.UtcNow.ToString("o"),
                message,
                severity
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Redis/log", eventPayload);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
               
                Console.WriteLine($"Event logging failed: {ex.Message}");
            }
        }
    }
}