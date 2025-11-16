using BackendApi.Data.Model.Enum;

namespace BackendApi.Data.Model.Incident;

public class Incident
{
    public string Id { get; set; }
    public string? Summary { get; set; }
    public string? Description { get; set; }
    public List<IncidentComment>? Comments { get; set; } = new();
    public DateTime CreateDate { get; set; }
    public DateTime? ChangeDate { get; set; }
    public DateTime? ClosedDate { get; set; }
    public IncidentStatus? Status { get; set; }
    public IncidentType? IncidentType { get; set; }
    public string? AssignedPerson { get; set; }
    public IncidentSeverity? Severity { get; set; }
    public string CustomerId { get; set; }
    public Guid UUId { get; set; }
}