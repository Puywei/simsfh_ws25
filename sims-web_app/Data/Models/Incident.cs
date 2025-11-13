namespace sims_web_app.Data.Models;

public class Incident
{
    public string? IncidentId { get; set; }
    
    public Guid IncidentGuid { get; set; }
    
    public string? Title { get; set; }
    
    public string? Description { get; set; }
    
    public string? Comment { get; set; }
    
    public IncidentSeverity Severity { get; set; }
    
    public IncidentState State { get; set; }
    
    public IncidentResponder? Responder { get; set; }
    
    public string Customer { get;  set; }
    
    public DateTime? CreatedOn { get; set; }
    
    public DateTime? UpdatedOn { get; set; }
    
    public DateTime? ClosedOn { get; set; }
}