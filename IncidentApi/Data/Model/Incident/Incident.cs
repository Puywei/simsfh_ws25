using Microsoft.EntityFrameworkCore;
using TestApi.Data.Model.Enum;

namespace TestApi.Data.Model.Incident;

public class Incident
{
    public string IncidentId { get; set; }
    public string? Summary { get; set; }
    public string? Description { get; set; }
    public List<IncidentComment>? Comments { get; set; } = new();
    public DateTime CreateDate { get; set; }
    public DateTime? UpdateDate { get; set; }
    public DateTime? ClosedDate { get; set; }
    public IncidentStatus? Status { get; set; }
    public IncidentType? IncidentType { get; set; }
    public string? AssignedPerson { get; set; }
    public IncidentSeverity? Severity { get; set; }
    public string? Author { get; set; }
    public Guid IncidentUUid { get; set; }
    
}