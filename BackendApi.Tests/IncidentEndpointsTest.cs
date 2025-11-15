using System.Net;
using System.Net.Http.Json;
using BackendApi.Data.Database;
using BackendApi.Data.Model.Enum;
using BackendApi.Data.Model.Incident;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BackendApi.Test
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
                    
                    db.Customers.AddRange(
                        new Customer { Id = "C-0001", CompanyName = "Customer 1", Email = "test1@test.at" },
                        new Customer { Id = "C-0002", CompanyName = "Customer 2", Email = "test2@test.at" }
                    );

                    db.Incidents.AddRange(
                        new Incident
                        { Id = "INC-1000", Summary = "Test incident 1", CustomerId = "C-0001"},
                        new Incident { Id = "INC-1001", Summary = "Test incident 2", CustomerId = "C-0002"}
                    );
                    db.SaveChanges();
                });
            });
        }

        [Fact]
        public async Task GetAllIncidents_ReturnsOkAndList()
        {
            HttpClient client = _factory.CreateClient();

            HttpResponseMessage response = await client.GetAsync("/api/v1/incidents");

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

            HttpResponseMessage response = await client.GetAsync($"/api/v1/incidents/{id}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            Incident incident = await response.Content.ReadFromJsonAsync<Incident>();
            incident.Should().NotBeNull();
            incident.Summary.Should().Be("Test incident 1");
        }

        [Fact]
        public async Task CreateIncident_ReturnsCreatedIncident()
        {
            HttpClient _client = _factory.CreateClient();
            
            Customer customer = new Customer { CompanyName = "Test Company", Email = "test@test.at", Id = "C-0004"};

            Incident newIncident = new Incident
            {
                Summary = "API Test Incident",
                Description = "Created via integration test",
                AssignedPerson = "Tester",
                Severity = IncidentSeverity.Medium,
                CustomerId = customer.Id,
                Status = IncidentStatus.Open,
                IncidentType = IncidentType.SecurityIncident
            };

            HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/incidents", newIncident);

            response.StatusCode.Should().Be(HttpStatusCode.Created);

            Incident createdIncident = await response.Content.ReadFromJsonAsync<Incident>();
            createdIncident.Should().NotBeNull();
            createdIncident!.Id.Should().StartWith("INC-");
            createdIncident.AssignedPerson.Should().Be("Tester");
        }

        [Fact]
        public async Task DeleteIncident_ShouldReturnNoContent_WhenIncidentExists()
        {
            HttpClient _client = _factory.CreateClient();
            
            Customer customer = new Customer { CompanyName = "Test Company", Email = "test@test.at", Id = "C-0006"};

            Incident newIncident = new Incident
            {
                Summary = "Incident to be deleted",
                Description = "This will be deleted in test",
                CustomerId = customer.Id,
                Severity = IncidentSeverity.Low,
                Status = IncidentStatus.Open,
                IncidentType = IncidentType.SecurityIncident
            };

            HttpResponseMessage postResponse = await _client.PostAsJsonAsync("/api/v1/incidents", newIncident);
            postResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            Incident createdIncident = await postResponse.Content.ReadFromJsonAsync<Incident>();
            createdIncident.Should().NotBeNull();

            HttpResponseMessage deleteResponse =
                await _client.DeleteAsync($"/api/v1/incidents/{createdIncident!.Id}");

            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task UpdateIncident_ShouldReturnOk()
        {
            HttpClient _client = _factory.CreateClient();
            
            Customer customer = new Customer { CompanyName = "Test Company", Email = "test@test.at", Id = "C-0008"};
            
            Incident newIncident = new Incident
            {
                Summary = "Original Summary",
                Description = "Original Description",
                CustomerId = customer.Id,
                AssignedPerson = "Original Assignee",
                Status = IncidentStatus.Open,
                Severity = IncidentSeverity.Low,
                IncidentType = IncidentType.SecurityIncident
            };

            HttpResponseMessage postResponse = await _client.PostAsJsonAsync("/api/v1/incidents", newIncident);
            postResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            Incident createdIncident = await postResponse.Content.ReadFromJsonAsync<Incident>();
            
            Incident updatedIncident = new Incident
            {
                Summary = "Updated Summary",
                Description = "Updated Description",
                AssignedPerson = "Updated Assignee",
                Status = IncidentStatus.onProgress,
                Severity = IncidentSeverity.High,
                IncidentType = IncidentType.SecurityIncident,
                ClosedDate = DateTime.Now
            };
            
            HttpResponseMessage putResponse = await _client.PutAsJsonAsync($"/api/v1/incidents/{createdIncident.Id}", updatedIncident);
            
            putResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            Incident returnedIncident = await putResponse.Content.ReadFromJsonAsync<Incident>();
            returnedIncident.Should().NotBeNull();
            returnedIncident!.Summary.Should().Be(updatedIncident.Summary);
            returnedIncident.Description.Should().Be(updatedIncident.Description);
            returnedIncident.AssignedPerson.Should().Be(updatedIncident.AssignedPerson);
            returnedIncident.Status.Should().Be(updatedIncident.Status);
            returnedIncident.Severity.Should().Be(updatedIncident.Severity);
        }

        [Fact]
        public async Task UpdateIncident_ShouldReturnNotFound_WhenIncidentDoesNotExist()
        {
            HttpClient _client = _factory.CreateClient();
            

            
            string existingIncident = "INC-1101";
            
            Incident updatedIncident = new Incident
            {
                Summary = "Updated Summary",
                Description = "Updated Description",
                AssignedPerson = "Updated Assignee",
                Status = IncidentStatus.onProgress,
                Severity = IncidentSeverity.High,
                IncidentType = IncidentType.SecurityIncident,
                ClosedDate = DateTime.Now
            };

            HttpResponseMessage putResponse = await _client.PutAsJsonAsync($"/api/v1/incidents/{existingIncident}", updatedIncident);
            
            putResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}