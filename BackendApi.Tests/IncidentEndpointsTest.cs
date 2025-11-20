using System.Net;
using System.Net.Http.Json;
using BackendApi.Data.Database;
using BackendApi.Data.Model.Customer;
using BackendApi.Data.Model.Enum;
using BackendApi.Data.Model.Incident;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BackendApi.Test;

public class IncidentEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public IncidentEndpointsTests(WebApplicationFactory<Program> factory)
    {
        Environment.SetEnvironmentVariable("TESTING", "true");

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor =
                    services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<MsSqlDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<MsSqlDbContext>(options => { options.UseInMemoryDatabase("TestDb"); });

                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<MsSqlDbContext>();
                db.Database.EnsureCreated();

                db.Incidents.AddRange(
                    new Incident { Id = "INC-1100", Summary = "Test incident 1", CustomerId = "C-0001" },
                    new Incident { Id = "INC-1101", Summary = "Test incident 2", CustomerId = "C-0002" }
                );
                db.SaveChanges();
            });
        });
    }


    [Fact]
    public async Task GetAllIncidents_ReturnsOkAndList()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/incidents");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var incidents = await response.Content.ReadFromJsonAsync<List<Incident>>();
        incidents.Should().NotBeNull();
        incidents.Should().HaveCount(2);
        incidents![0].Summary.Should().Be("Test incident 1");
    }


    [Fact]
    public async Task GetIncidentById_ReturnsOk()
    {
        var id = "INC-1100";
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/incidents/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var incident = await response.Content.ReadFromJsonAsync<Incident>();
        incident.Should().NotBeNull();
        incident.Summary.Should().Be("Test incident 1");
    }

    [Fact]
    public async Task CreateIncident_ReturnsCreatedIncident()
    {
        var _client = _factory.CreateClient();

        var customer = new Customer { CompanyName = "Test Company", Email = "test@test.at", Id = "C-0004" };

        var newIncident = new Incident
        {
            Summary = "API Test Incident",
            Description = "Created via integration test",
            AssignedPerson = 2,
            Severity = IncidentSeverity.Medium,
            CustomerId = customer.Id,
            Status = IncidentStatus.Open,
            IncidentType = IncidentType.SecurityIncident
        };

        var response = await _client.PostAsJsonAsync("/api/v1/incidents", newIncident);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdIncident = await response.Content.ReadFromJsonAsync<Incident>();
        createdIncident.Should().NotBeNull();
        createdIncident!.Id.Should().StartWith("INC-");
        createdIncident.AssignedPerson.Should().Be(2);
    }

    [Fact]
    public async Task DeleteIncident_ShouldReturnNoContent_WhenIncidentExists()
    {
        var _client = _factory.CreateClient();

        var customer = new Customer { CompanyName = "Test Company", Email = "test@test.at", Id = "C-0006" };

        var newIncident = new Incident
        {
            Summary = "Incident to be deleted",
            Description = "This will be deleted in test",
            CustomerId = customer.Id,
            Severity = IncidentSeverity.Low,
            Status = IncidentStatus.Open,
            IncidentType = IncidentType.SecurityIncident
        };

        var postResponse = await _client.PostAsJsonAsync("/api/v1/incidents", newIncident);
        postResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdIncident = await postResponse.Content.ReadFromJsonAsync<Incident>();
        createdIncident.Should().NotBeNull();

        var deleteResponse =
            await _client.DeleteAsync($"/api/v1/incidents/{createdIncident.Id}");

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateIncident_ShouldReturnOk()
    {
        var _client = _factory.CreateClient();

        var customer = new Customer { CompanyName = "Test Company", Email = "test@test.at", Id = "C-0008" };

        var newIncident = new Incident
        {
            Summary = "Original Summary",
            Description = "Original Description",
            CustomerId = customer.Id,
            AssignedPerson = 2,
            Status = IncidentStatus.Open,
            Severity = IncidentSeverity.Low,
            IncidentType = IncidentType.SecurityIncident
        };

        var postResponse = await _client.PostAsJsonAsync("/api/v1/incidents", newIncident);
        postResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdIncident = await postResponse.Content.ReadFromJsonAsync<Incident>();

        var updatedIncident = new Incident
        {
            Summary = "Updated Summary",
            Description = "Updated Description",
            AssignedPerson = 2,
            Status = IncidentStatus.onProgress,
            Severity = IncidentSeverity.High,
            IncidentType = IncidentType.SecurityIncident,
            ClosedDate = DateTime.Now
        };

        var putResponse = await _client.PutAsJsonAsync($"/api/v1/incidents/{createdIncident.Id}", updatedIncident);

        putResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var returnedIncident = await putResponse.Content.ReadFromJsonAsync<Incident>();
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
        var _client = _factory.CreateClient();

        var existingIncident = "INC-1001";

        var updatedIncident = new Incident
        {
            Summary = "Updated Summary",
            Description = "Updated Description",
            AssignedPerson = 2,
            Status = IncidentStatus.onProgress,
            Severity = IncidentSeverity.High,
            IncidentType = IncidentType.SecurityIncident,
            ClosedDate = DateTime.Now
        };

        var putResponse = await _client.PutAsJsonAsync($"/api/v1/incidents/{existingIncident}", updatedIncident);

        putResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}