using System.ComponentModel.DataAnnotations;

namespace TestApi.Data.Model.Incident;

public class IncidentComment
{
    [Key]
    public Guid CommentId { get; set; }
    
    public int CommentOrder { get; internal set; }
    
    public Incident Incident { get; set; }
    public string IncidentId { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreateDate { get; set; }
    public string? UserName { get; set; }
    public string? Comment { get; set; }
}