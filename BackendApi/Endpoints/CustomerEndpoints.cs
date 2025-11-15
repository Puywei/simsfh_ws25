using BackendApi.Data.Database;
using BackendApi.Data.Model.Customer;
using BackendApi.Data.Model.Incident;
using BackendApi.Data.Services;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Endpoints;

public static class CustomerEndpoints
{
     public static void MapCustomerEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder customers = routes.MapGroup("/api/v1/customers");
        customers.MapGet("", async (MsSqlDbContext dbContext) =>
            {
                List<Customer> customers = await dbContext.Customers.ToListAsync();
                return Results.Ok(customers);
            })
            .WithName("GetCustomers")
            .WithDescription("Returns all customers");

        customers.MapGet("/{id}", async (MsSqlDbContext dbContext, string id) =>
            {
                Customer? customer = await dbContext.Customers.FirstOrDefaultAsync(c => c.Id == id);
                if (customer is null)
                    return Results.NotFound();
                
                return Results.Ok(customer);
            })
            .WithName("GetCustomerById")
            .WithDescription("Returns a customer by id");

        customers.MapDelete("/{id}", async (MsSqlDbContext dbContext, string id, RedisLogService logService) =>
            {
                Customer? customer = await dbContext.Customers.FirstOrDefaultAsync(c => c.Id == id);
                if (customer is null)
                    return Results.NotFound();

                dbContext.Customers.Remove(customer);
                await dbContext.SaveChangesAsync();
                await logService.LogAsync($"[{DateTime.Now}]Customer deleted: {customer.Id}, CustomerId: {customer.UUId}, CustomerId: {customer.CompanyName}");

                return Results.NoContent();
            })
            .WithName("DeleteCustomer")
            .WithDescription("Deletes a Customer");

        customers.MapPost("", async (MsSqlDbContext dbContext, Customer newCustomer, RedisLogService logService) =>
            {
                if (newCustomer == null || 
                    newCustomer.CompanyName == null ||
                    newCustomer.Email == null ||
                    newCustomer.CompanyName == "" || 
                    newCustomer.Email == "")
                    return Results.BadRequest("Invalid request body");
                
                // overwrite following attributes
                newCustomer.Id = GenerateIdService.CustomerId(dbContext);
                newCustomer.UUId = Guid.NewGuid();
                newCustomer.CreateDate = DateTime.Now;
                newCustomer.ChangeDate = DateTime.Now;
                dbContext.Customers.Add(newCustomer);
                
                await dbContext.SaveChangesAsync();
                await logService.LogAsync($"[{DateTime.Now}]Create customer {newCustomer.Id}, CustomerId: {newCustomer.UUId},  Company: {newCustomer.CompanyName}");
                
                return Results.Created($"/api/incidents/{newCustomer.Id}", newCustomer);
            })
            .WithName("CreateCustomer")
            .WithDescription("Create a new Customer");
        
        customers.MapPut("/{existingCustomerId}", async (MsSqlDbContext dbContext, string existingCustomerId, Customer updatedCustomer, RedisLogService logService) =>
            {
                if (!dbContext.Customers.Any(c => c.Id == existingCustomerId) || updatedCustomer.CompanyName is null || updatedCustomer.Email is null || updatedCustomer.CompanyName == "" || updatedCustomer.Email == "")
                    return Results.BadRequest("Invalid request body");
                
                Customer? existingCustomer = await dbContext.Customers.FirstOrDefaultAsync(c => c.Id == existingCustomerId);
                
                existingCustomer.CompanyName = updatedCustomer.CompanyName;
                existingCustomer.Email = updatedCustomer.Email;
                existingCustomer.Active = updatedCustomer.Active;
                existingCustomer.Address = updatedCustomer.Address;
                existingCustomer.City = updatedCustomer.City;
                existingCustomer.Country = updatedCustomer.Country;
                existingCustomer.PhoneNumber = updatedCustomer.PhoneNumber;
                existingCustomer.State = updatedCustomer.State;
                existingCustomer.ZipCode = updatedCustomer.ZipCode;
                existingCustomer.ChangeDate = DateTime.Now;
                
                await dbContext.SaveChangesAsync();
                await logService.LogAsync($"[{DateTime.Now}]Updated customer {existingCustomer.Id}, CustomerId: {existingCustomer.UUId},  Company: {existingCustomer.CompanyName}");

                
                return Results.Ok(existingCustomer);
            })
            .WithName("UpdateCustomer")
            .WithDescription("Update an existing customer");
    }
}