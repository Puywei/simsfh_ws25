using BackendApi.Data.Database;
using BackendApi.Data.Model.Incident;
using BackendApi.Data.Services;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Endpoints;

public static class IncidentEndpoints
{
    public static void MapIncidentEndpoints(this IEndpointRouteBuilder routes )
    {
        RouteGroupBuilder incidents = routes.MapGroup("/api/v1/incidents");
        incidents.MapGet("", async (MsSqlDbContext dbContext) =>
            {
                List<Incident> incidentList = await dbContext.Incidents.ToListAsync();
                return Results.Ok(incidentList);
            })
            .WithName("GetAllIncidents")
            .WithDescription("Returns all incidents");

        incidents.MapGet("/{id}", async (MsSqlDbContext dbContext, string id) =>
            {
                Incident? incident = await dbContext.Incidents.FirstOrDefaultAsync(inc => inc.Id == id);
                return Results.Ok(incident);
            })
            .WithName("GetIncidentById")
            .WithDescription("Returns a incident by id");

        incidents.MapDelete("/{id}", async (MsSqlDbContext dbContext, string id) =>
            {
                Incident? incident = await dbContext.Incidents.FirstOrDefaultAsync(inc => inc.Id == id);
                if (incident is null)
                    return Results.NotFound();

                dbContext.Incidents.Remove(incident);
                await dbContext.SaveChangesAsync();
                return Results.NoContent();
            })
            .WithName("DeleteIncident")
            .WithDescription("Deletes a Incident");

        incidents.MapPost("", async (MsSqlDbContext dbContext, Incident newIncident, RedisLogService logService) =>
            {
                if(newIncident is null ||  newIncident.CustomerId == null)
                    return Results.BadRequest("Invalid request body");
                
                // Überschreibt die Settings egal was man geschickt bekommt
                newIncident.Id = GenerateIdService.IncidentId(dbContext);
                newIncident.UUId = Guid.NewGuid();
                newIncident.CreateDate = DateTime.Now;
                newIncident.ChangeDate = DateTime.Now;
                
                dbContext.Incidents.Add(newIncident);
                await dbContext.SaveChangesAsync();
                await logService.LogAsync($"[{DateTime.Now}]Create incident {newIncident.Id}, incidentId: {newIncident.UUId}");

                
                return Results.Created($"/{newIncident.Id}", newIncident);
            })
            .WithName("CreateIncident")
            .WithDescription("Create a new Incident");
        
        incidents.MapPut("/{existingIncidentIncidentId}", async (MsSqlDbContext dbContext, string existingIncidentIncidentId, Incident updatedIncident) =>
            {
                if (updatedIncident == null)
                    return Results.BadRequest("Invalid request body");
                

                Incident existingIncident = await dbContext.Incidents.FindAsync(existingIncidentIncidentId);
                if (existingIncident == null)
                    return Results.BadRequest();
                
                if (updatedIncident.AssignedPerson != null)
                    existingIncident.AssignedPerson = updatedIncident.AssignedPerson;

                if (updatedIncident.CustomerId != null)
                    existingIncident.CustomerId = updatedIncident.CustomerId;

                if (updatedIncident.ClosedDate != null)
                    existingIncident.ClosedDate = updatedIncident.ClosedDate;

                if (updatedIncident.Description != null)
                    existingIncident.Description = updatedIncident.Description;

                if (updatedIncident.IncidentType != null)
                    existingIncident.IncidentType = updatedIncident.IncidentType;

                if (updatedIncident.Severity != null)
                    existingIncident.Severity = updatedIncident.Severity;

                if (updatedIncident.Status != null)
                    existingIncident.Status = updatedIncident.Status;

                if (updatedIncident.Summary != null)
                existingIncident.Summary = updatedIncident.Summary;

                existingIncident.ChangeDate = DateTime.UtcNow;

                await dbContext.SaveChangesAsync();

                return Results.Ok(existingIncident);
            })
            .WithName("UpdateIncident")
            .WithDescription("Update a existingIncident Incident");
    }
}