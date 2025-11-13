using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TestApi.Data.Database;
using TestApi.Data.Model.Enum;
using TestApi.Data.Model.Incident;
using Xunit;

namespace TestApi.Tests
{
    public class IncidentEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public IncidentEndpointsTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor =
                        services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApiDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    services.AddDbContext<ApiDbContext>(options => { options.UseInMemoryDatabase("TestDb"); });

                    var sp = services.BuildServiceProvider();
                    using var scope = sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
                    db.Database.EnsureDeleted();
                    db.Database.EnsureCreated();

                    db.Incidents.AddRange(
                        new Incident { IncidentId = "INC-1000", Summary = "Test incident 1" },
                        new Incident { IncidentId = "INC-1001", Summary = "Test incident 2" }
                    );
                    db.SaveChanges();
                });
            });
        }

        [Fact]
        public async Task GetAllIncidents_ReturnsOkAndList()
        {
            HttpClient client = _factory.CreateClient();

            HttpResponseMessage response = await client.GetAsync("/api/incidents");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            List<Incident> incidents = await response.Content.ReadFromJsonAsync<List<Incident>>();
            incidents.Should().NotBeNull();
            incidents.Should().HaveCount(2);
            incidents![0].Summary.Should().Be("Test incident 1");
        }


        [Fact]
        public async Task GetIncidentById_ReturnsOk()
        {
            string id = "INC-1000";
            HttpClient client = _factory.CreateClient();

            HttpResponseMessage response = await client.GetAsync($"/api/incidents/{id}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            Incident incident = await response.Content.ReadFromJsonAsync<Incident>();
            incident.Should().NotBeNull();
            incident.Summary.Should().Be("Test incident 1");
        }

        [Fact]
        public async Task CreateIncident_ReturnsCreatedIncident()
        {
            HttpClient _client = _factory.CreateClient();

            Incident newIncident = new Incident
            {
                Summary = "API Test Incident",
                Description = "Created via integration test",
                AssignedPerson = "Tester",
                Severity = IncidentSeverity.Medium,
                Author = "UnitTest",
                Status = IncidentStatus.Open,
                IncidentType = IncidentType.SecurityIncident
            };

            HttpResponseMessage response = await _client.PostAsJsonAsync("/api/incidents", newIncident);

            response.StatusCode.Should().Be(HttpStatusCode.Created);

            Incident createdIncident = await response.Content.ReadFromJsonAsync<Incident>();
            createdIncident.Should().NotBeNull();
            createdIncident!.IncidentId.Should().StartWith("INC-");
            createdIncident.AssignedPerson.Should().Be("Tester");
        }

        [Fact]
        public async Task DeleteIncident_ShouldReturnNoContent_WhenIncidentExists()
        {
            HttpClient _client = _factory.CreateClient();

            Incident newIncident = new Incident
            {
                Summary = "Incident to be deleted",
                Description = "This will be deleted in test",
                Author = "TestUser",
                Severity = IncidentSeverity.Low,
                Status = IncidentStatus.Open,
                IncidentType = IncidentType.SecurityIncident
            };

            HttpResponseMessage postResponse = await _client.PostAsJsonAsync("/api/incidents", newIncident);
            postResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            Incident createdIncident = await postResponse.Content.ReadFromJsonAsync<Incident>();
            createdIncident.Should().NotBeNull();

            HttpResponseMessage deleteResponse =
                await _client.DeleteAsync($"/api/incidents/{createdIncident!.IncidentId}");

            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task UpdateIncident_ShouldReturnOk_WhenIncidentExists()
        {
            HttpClient _client = _factory.CreateClient();
            
            Incident newIncident = new Incident
            {
                IncidentId = "INC-9999",
                Summary = "Original Summary",
                Description = "Original Description",
                Author = "Original Author",
                AssignedPerson = "Original Assignee",
                Status = IncidentStatus.Open,
                Severity = IncidentSeverity.Low,
                IncidentType = IncidentType.SecurityIncident
            };

            HttpResponseMessage postResponse = await _client.PostAsJsonAsync("/api/incidents", newIncident);
            postResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            
            Incident updatedIncident = new Incident
            {
                Summary = "Updated Summary",
                Description = "Updated Description",
                Author = "Updated Author",
                AssignedPerson = "Updated Assignee",
                Status = IncidentStatus.onProgress,
                Severity = IncidentSeverity.High,
                IncidentType = IncidentType.SecurityIncident,
                ClosedDate = DateTime.Now
            };

            HttpResponseMessage putResponse = await _client.PutAsJsonAsync($"/api/incidents/{newIncident.IncidentId}", updatedIncident);
            
            putResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            Incident returnedIncident = await putResponse.Content.ReadFromJsonAsync<Incident>();
            returnedIncident.Should().NotBeNull();
            returnedIncident!.Summary.Should().Be(updatedIncident.Summary);
            returnedIncident.Description.Should().Be(updatedIncident.Description);
            returnedIncident.AssignedPerson.Should().Be(updatedIncident.AssignedPerson);
            returnedIncident.Author.Should().Be(updatedIncident.Author);
            returnedIncident.Status.Should().Be(updatedIncident.Status);
            returnedIncident.Severity.Should().Be(updatedIncident.Severity);
        }

        [Fact]
        public async Task UpdateIncident_ShouldReturnNotFound_WhenIncidentDoesNotExist()
        {
            HttpClient _client = _factory.CreateClient();
            
            var updatedIncident = new
            {
                Summary = "Non-existent",
                Description = "Non-existent",
                Author = "Nobody",
                AssignedPerson = "Nobody",
                Status = "Open",
                Severity = "Low",
                IncidentType = "SecurityIncident"
            };

            var response = await _client.PutAsJsonAsync("/api/incidents/INC-1011", updatedIncident);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}