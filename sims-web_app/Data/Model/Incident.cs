using sims_web_app.Data.Model.Enum;

namespace sims_web_app.Data.Model;

public class Incident
{
    public Incident()
    {
        Id = "INC";
        UUId = new Guid();
        CreateDate = new DateTime();
        ChangeDate = new DateTime();
        ClosedDate = new DateTime();
    }
    
    public string? Id { get; set; }
    public string? Summary { get; set; }
    public string? Description { get; set; }
    public List<IncidentComment>? Comments { get; set; } = new();
    public DateTime? CreateDate { get; set; }
    public DateTime? ChangeDate { get; set; }
    public DateTime? ClosedDate { get; set; }
    public IncidentStatus? Status { get; set; }
    public IncidentType? IncidentType { get; set; }
    public int AssignedPerson { get; set; }
    public string? AssignedPersonName { get; set; }
    public IncidentSeverity? Severity { get; set; }
    public string CustomerId { get; set; }
    public Guid? UUId { get; set; }
}