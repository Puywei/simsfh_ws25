using BackendApi.Data.Database;
using BackendApi.Data.Model.Incident;
using BackendApi.Data.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class IncidentsController : ControllerBase
{
    private readonly MsSqlDbContext _dbContext;

    public IncidentsController(MsSqlDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Incident>>> GetAll()
    {
        var incidents = await _dbContext.Incidents.ToListAsync();
        return Ok(incidents);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<Incident>> GetById(string id)
    {
        var incident = await _dbContext.Incidents.FirstOrDefaultAsync(x => x.Id == id);
        if (incident == null)
            return NotFound();

        return Ok(incident);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var incident = await _dbContext.Incidents.FirstOrDefaultAsync(x => x.Id == id);
        if (incident == null)
            return NotFound();

        _dbContext.Incidents.Remove(incident);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }
    
    [HttpPost]
    public async Task<ActionResult<Incident>> Create(Incident newIncident)
    {
        if (newIncident.CustomerId == null)
            return BadRequest("Invalid request body");

        newIncident.Id = GenerateIdService.IncidentId(_dbContext);
        newIncident.UUId = Guid.NewGuid();
        newIncident.CreateDate = DateTime.Now;
        newIncident.ChangeDate = DateTime.Now;
        newIncident.Comments = new List<IncidentComment>();

        _dbContext.Incidents.Add(newIncident);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = newIncident.Id }, newIncident);
    }
    
    [HttpPut("{id}")]
    public async Task<ActionResult<Incident>> Update(string id, Incident updated)
    {
        if (updated == null)
            return BadRequest("Invalid request body");

        var existingIncident = await _dbContext.Incidents.FindAsync(id);
        if (existingIncident == null)
            return NotFound();
        
        if (updated.AssignedPerson != null)
            existingIncident.AssignedPerson = updated.AssignedPerson;

        if (updated.CustomerId != null)
            existingIncident.CustomerId = updated.CustomerId;

        if (updated.ClosedDate != null)
            existingIncident.ClosedDate = updated.ClosedDate;

        if (updated.Description != null)
            existingIncident.Description = updated.Description;

        if (updated.IncidentType != null)
            existingIncident.IncidentType = updated.IncidentType;

        if (updated.Severity != null)
            existingIncident.Severity = updated.Severity;

        if (updated.Status != null)
            existingIncident.Status = updated.Status;

        if (updated.Summary != null)
            existingIncident.Summary = updated.Summary;

        existingIncident.ChangeDate = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return Ok(existingIncident);
    }
}
