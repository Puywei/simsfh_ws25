namespace sims_web_app.Data.Model;

public class LogEntry
{

    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Path { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string? Body { get; set; }
    public string? User { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}