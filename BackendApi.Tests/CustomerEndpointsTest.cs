using System.Net;
using System.Net.Http.Json;
using BackendApi.Data.Database;
using BackendApi.Data.Model.Customer;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
                    var descriptors = services
                        .Where(d => d.ServiceType == typeof(DbContextOptions<MsSqlDbContext>) ||
                                    d.ImplementationType == typeof(MsSqlDbContext))
                        .ToList();
                    foreach (var d in descriptors)
                        services.Remove(d);
                    
                    services.AddDbContext<MsSqlDbContext>(options =>
                        options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
                });
            });
            
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MsSqlDbContext>();
            db.Database.EnsureCreated();

            db.Customers.AddRange(
                new Customer { Id = "C-1000", CompanyName = "Customer KG", Email = "customer@kg.at", UUId = Guid.NewGuid() },
                new Customer { Id = "C-1001", CompanyName = "Customer GmbH", Email = "customer@gmbh.at", UUId = Guid.NewGuid() }
            );
            db.SaveChanges();
        }


        [Fact]
        public async Task GetAllCustomers_ReturnsOk()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/v1/Customers");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var customers = await response.Content.ReadFromJsonAsync<List<Customer>>();
            customers.Should().NotBeNull();
            customers.Should().HaveCount(2);
            customers![0].CompanyName.Should().Be("Customer KG");
        }

        [Fact]
        public async Task GetCustomerById_ReturnsOk()
        {
            var client = _factory.CreateClient();

            var id = "C-1000";
            var response = await client.GetAsync($"/api/v1/Customers/{id}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var customer = await response.Content.ReadFromJsonAsync<Customer>();
            customer.Should().NotBeNull();
            customer.CompanyName.Should().Be("Customer KG");
        }

        [Fact]
        public async Task GetCustomerById_ReturnsNotFound()
        {
            var client = _factory.CreateClient();

            var id = "C-1009";
            var response = await client.GetAsync($"/api/v1/Customers/{id}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreateCustomer_ReturnsOk()
        {
            var client = _factory.CreateClient();

            Customer customer = new()
            {
                CompanyName = "Customer KG12",
                Email = "customer@cus.at"
            };
            var response = await client.PostAsJsonAsync("/api/v1/Customers/", customer);
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var newCustomer = await response.Content.ReadFromJsonAsync<Customer>();

            newCustomer.Should().NotBeNull();
            newCustomer.CompanyName.Should().Be("Customer KG12");
            newCustomer.Email.Should().Be("customer@cus.at");
        }

        [Fact]
        public async Task CreateCustomerWithId_ReturnsWithOtherIdOk()
        {
            var client = _factory.CreateClient();

            Customer customer = new()
            {
                Id = "C-1000",
                CompanyName = "Customer KG12",
                Email = "customer@cus.at"
            };
            var response = await client.PostAsJsonAsync("/api/v1/Customers/", customer);
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var newCustomer = await response.Content.ReadFromJsonAsync<Customer>();

            newCustomer.Should().NotBeNull();
            newCustomer.Id.Should().NotBe("C-1000");
            newCustomer.CompanyName.Should().Be("Customer KG12");
            newCustomer.Email.Should().Be("customer@cus.at");
        }

        [Fact]
        public async Task UpdateCustomer_ReturnsOk()
        {
            var client = _factory.CreateClient();

            var customerToUpdate = "C-1000";

            var customer = new Customer { CompanyName = "Updated Customer KG12", Email = "update mailaddress" };
            var response = await client.PutAsJsonAsync($"/api/v1/customers/{customerToUpdate}", customer);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var newCustomer = await response.Content.ReadFromJsonAsync<Customer>();

            newCustomer.Should().NotBeNull();
            newCustomer.Id.Should().Be("C-1000");
            newCustomer.CompanyName.Should().Be("Updated Customer KG12");
            newCustomer.Email.Should().Be("update mailaddress");
        }

        [Fact]
        public async Task UpdateCustomerWrongId_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();

            var customerToUpdate = "C-1000";

            var customer = new Customer(); // { CompanyName = "Updated Customer KG12", Email = "update mailaddress" };

            var response = await client.PutAsJsonAsync($"/api/v1/Customers/{customerToUpdate}", customer);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}