using System.Net;
using System.Net.Http.Json;
using BackendApi.Data.Database;
using BackendApi.Data.Model.Customer;
using BackendApi.Data.Model.Incident;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using Xunit;

namespace BackendApi.Test;

public class CustomerEndpointsTest
{
    public class CustomerEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
    {

        private readonly WebApplicationFactory<Program> _factory;

        public CustomerEndpointsTests(WebApplicationFactory<Program> factory)
        {
            Environment.SetEnvironmentVariable("TESTING", "true");

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
                        new Customer { Id = "C-1000", CompanyName = "Customer KG", Email = "customer@kg.at", UUId = Guid.NewGuid()},
                        new Customer { Id = "C-1001", CompanyName = "Customer GmbH", Email = "customer@gmbh.at", UUId = Guid.NewGuid() }
                    );
                    db.SaveChanges();
                });
            });
        }
        
        [Fact]
        public async Task GetAllCustomers_ReturnsOk()
        {
            HttpClient client = _factory.CreateClient();

            HttpResponseMessage response = await client.GetAsync("/api/v1/customers");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            List<Customer> customers = await response.Content.ReadFromJsonAsync<List<Customer>>();
            customers.Should().NotBeNull();
            customers.Should().HaveCount(2);
            customers![0].CompanyName.Should().Be("Customer KG");
        }
        
        [Fact]
        public async Task GetCustomerById_ReturnsOk()
        {
            HttpClient client = _factory.CreateClient();
            
            string id = "C-1000";
            HttpResponseMessage response = await client.GetAsync($"/api/v1/customers/{id}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            Customer customer = await response.Content.ReadFromJsonAsync<Customer>();
            customer.Should().NotBeNull();
            customer.CompanyName.Should().Be("Customer KG");
        }
        
        [Fact]
        public async Task GetCustomerById_ReturnsNotFound()
        {
            HttpClient client = _factory.CreateClient();
            
            string id = "C-1009";
            HttpResponseMessage response = await client.GetAsync($"/api/v1/customers/{id}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        
        [Fact]
        public async Task CreateCustomer_ReturnsOk()
        {
            HttpClient client = _factory.CreateClient();
            
            string id = "C-1009";
            HttpResponseMessage response = await client.PostAsync($"/api/v1/customers/{id}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}