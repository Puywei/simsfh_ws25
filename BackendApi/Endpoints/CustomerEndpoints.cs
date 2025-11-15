using BackendApi.Data.Database;
using BackendApi.Data.Model.Incident;
using BackendApi.Data.Services;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Endpoints;

public static class CustomerEndpoints
{
     public static void MapCustomerEndpoints(this WebApplication app)
    {
        app.MapGet("/api/customers", async (ApiDbContext dbContext) =>
            {
                List<Customer> customers = await dbContext.Customers.ToListAsync();
                return Results.Ok(customers);
            })
            .WithName("GetCustomers")
            .WithDescription("Returns all customers");

        app.MapGet("/api/customers/{id}", async (ApiDbContext dbContext, string id) =>
            {
                Customer? customer = await dbContext.Customers.FirstOrDefaultAsync(c => c.Id == id);
                if (customer is null)
                    return Results.NotFound();
                
                return Results.Ok(customer);
            })
            .WithName("GetCustomerById")
            .WithDescription("Returns a customer by id");

        app.MapDelete("/api/customers/{id}", async (ApiDbContext dbContext, string id) =>
            {
                Customer? customer = await dbContext.Customers.FirstOrDefaultAsync(c => c.Id == id);
                if (customer is null)
                    return Results.NotFound();

                dbContext.Customers.Remove(customer);
                await dbContext.SaveChangesAsync();
                return Results.NoContent();
            })
            .WithName("DeleteCustomer")
            .WithDescription("Deletes a Customer");

        app.MapPost("/api/customers", async (ApiDbContext dbContext, Customer newCustomer) =>
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
                
                return Results.Created($"/api/incidents/{newCustomer.Id}", newCustomer);
            })
            .WithName("CreateCustomer")
            .WithDescription("Create a new Customer");
        
        app.MapPut("/api/customers/{existingIncidentId}", async (ApiDbContext dbContext, string existingCustomerId, Customer updatedCustomer) =>
            {
                if (existingCustomerId == null || updatedCustomer == null)
                    return Results.BadRequest("Invalid request body");

                Customer existingCustomer = await dbContext.Customers.FirstOrDefaultAsync(c => c.Id == existingCustomerId);
                if (existingCustomerId == null)
                    return Results.NotFound($"Incident with id {existingCustomerId} does not exist");
                
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
                
                return Results.Ok(existingCustomer);
            })
            .WithName("UpdateCustomer")
            .WithDescription("Update an existing customer");
    }
}