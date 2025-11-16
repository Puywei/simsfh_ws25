using BackendApi.Data.Database;
using BackendApi.Data.Model.Customer;
using BackendApi.Data.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly MsSqlDbContext _dbContext;

    public CustomersController(MsSqlDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get all Custoemrs",
        Description = "Returns a list with all customers in the database."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "List of Customers found")]
    public async Task<IActionResult> GetCustomers()
    {
        List<Customer> customers = await _dbContext.Customers.ToListAsync();
        return Ok(customers);
    }
    
    [HttpGet("{id}")]
    [SwaggerOperation(
        Summary = "Get incident by ID",
        Description = "Returns a single incident identified by the unique incident ID."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Incident found")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Incident not found")]
    public async Task<ActionResult<Customer>> GetCustomerById(string id)
    {
        var customer = await _dbContext.Customers.FirstOrDefaultAsync(c => c.Id == id);
        if (customer is null)
            return NotFound();

        return Ok(customer);
    }
    
    [HttpDelete("{id}")]
    [SwaggerOperation(
        Summary = "Delete a Customer by ID",
        Description = "Delets a Customer by ID."
    )]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Customer deleted")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Customer ID not found")]
    public async Task<IActionResult> DeleteCustomer(string id)
    {
        var customer = await _dbContext.Customers.FirstOrDefaultAsync(c => c.Id == id);
        if (customer is null)
            return NotFound();

        _dbContext.Customers.Remove(customer);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }
    
    [HttpPost]
    [SwaggerOperation(
        Summary = "Create a Customer",
        Description = "Create a Customer, ID will be generated automatically"
    )]
    [SwaggerResponse(StatusCodes.Status201Created, "Customer created")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Customer can't be created")]
    public async Task<IActionResult> CreateCustomer([FromBody] Customer newCustomer)
    {
        if (string.IsNullOrWhiteSpace(newCustomer.CompanyName) ||
            string.IsNullOrWhiteSpace(newCustomer.Email))
        {
            return BadRequest("Invalid request body TEST");
        }
        
        newCustomer.Id = GenerateIdService.CustomerId(_dbContext);
        newCustomer.UUId = Guid.NewGuid();
        newCustomer.CreateDate = DateTime.Now;
        newCustomer.ChangeDate = DateTime.Now;

        _dbContext.Customers.Add(newCustomer);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCustomerById), new { id = newCustomer.Id }, newCustomer);
    }
    
    [HttpPut("{existingCustomerId}")]
    [SwaggerOperation(
        Summary = "Update a Customer by ID",
        Description = "Updates values from a Customer by ID."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Customer updated")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Customer ID not found")]
    public async Task<IActionResult> UpdateCustomer(string existingCustomerId, [FromBody] Customer updatedCustomer)
    {
        if (string.IsNullOrWhiteSpace(updatedCustomer.CompanyName) ||
            string.IsNullOrWhiteSpace(updatedCustomer.Email))
        {
            return BadRequest("Invalid request body");
        }

        var existingCustomer =
            await _dbContext.Customers.FirstOrDefaultAsync(c => c.Id == existingCustomerId);

        if (existingCustomer is null)
            return BadRequest("Invalid request body");

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

        await _dbContext.SaveChangesAsync();

        return Ok(existingCustomer);
    }
}
