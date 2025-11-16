using System.ComponentModel.DataAnnotations;

namespace BackendApi.Data.Model.LogData;

public class LogEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Path { get; set; }
    public string Method { get; set; }
    public string? Body { get; set; }
    public string? User { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}