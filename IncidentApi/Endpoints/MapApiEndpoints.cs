using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestApi.Data.Database;
using TestApi.Data.Model.Incident;
using TestApi.Data.Services;

namespace TestApi.Endpoints;

public static class MapApiEndpoints
{
    public static void MapIncidentEndpoints(this WebApplication app)
    {
        app.MapGet("/api/incidents", async (ApiDbContext dbContext) =>
            {
                List<Incident> incidents = await dbContext.Incidents.ToListAsync();
                return Results.Ok(incidents);
            })
            .WithName("GetAllIncidents")
            .WithDescription("Returns all incidents");

        app.MapGet("/api/incidents/{id}", async (ApiDbContext dbContext, string id) =>
            {
                Incident incident = await dbContext.Incidents.FirstOrDefaultAsync(inc => inc.IncidentId == id);
                return Results.Ok(incident);
            })
            .WithName("GetIncidentById")
            .WithDescription("Returns a incident by id");

        app.MapDelete("/api/incidents/{id}", async (ApiDbContext dbContext, string id) =>
            {
                Incident incident = await dbContext.Incidents.FirstOrDefaultAsync(inc => inc.IncidentId == id);
                if (incident is null)
                    return Results.NotFound();

                dbContext.Incidents.Remove(incident);
                await dbContext.SaveChangesAsync();
                return Results.NoContent();
            })
            .WithName("DeleteIncident")
            .WithDescription("Deletes a Incident");

        app.MapPost("/api/incidents", async (ApiDbContext dbContext, Incident newIncident) =>
            {
                // Überschreibt die Settings egal was man geschickt bekommt
                newIncident.IncidentId = IncidentService.GenerateIncidentId(dbContext);
                newIncident.IncidentUUid = Guid.NewGuid();
                newIncident.CreateDate = DateTime.Now;
                newIncident.UpdateDate = DateTime.Now;
                
                dbContext.Incidents.Add(newIncident);
                await dbContext.SaveChangesAsync();
                
                return Results.Created($"/api/incidents/{newIncident.IncidentId}", newIncident);
            })
            .WithName("CreateIncident")
            .WithDescription("Create a new Incident");
        
        app.MapPut("/api/incidents/{id}", async (ApiDbContext dbContext, string existingIncidentId, Incident updatedIncident) =>
            {
                if (updatedIncident == null)
                    return Results.BadRequest("Invalid request body");

                var existingIncident = await dbContext.Incidents.FirstOrDefaultAsync(i => i.IncidentId == existingIncidentId);
                if (existingIncident == null)
                    return Results.NotFound($"Incident with id {existingIncidentId} does not exist");
                
                existingIncident.AssignedPerson = updatedIncident.AssignedPerson;
                existingIncident.Author = updatedIncident.Author;
                existingIncident.ClosedDate = updatedIncident.ClosedDate;
                existingIncident.Description = updatedIncident.Description;
                existingIncident.IncidentType = updatedIncident.IncidentType;
                existingIncident.Severity = updatedIncident.Severity;
                existingIncident.Status = updatedIncident.Status;
                existingIncident.Summary = updatedIncident.Summary;
                existingIncident.UpdateDate = DateTime.Now;
                
                await dbContext.SaveChangesAsync();
                
                return Results.Ok(existingIncident);
            })
            .WithName("UpdateIncident")
            .WithDescription("Update a existing Incident");
    }
}