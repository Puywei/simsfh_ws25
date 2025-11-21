using Newtonsoft.Json;
using sims_web_app.Data.Model.Enum;

namespace sims_web_app.Data.Model;

public class LogEntry
{
    [JsonProperty("logid")]
    public string LogId { get; set; }
    [JsonProperty("timestamp")]
    public DateTime Timestamp { get; set; }
    [JsonProperty("message")]
    public string Message { get; set; }
    [JsonProperty("severity")]
    public int Severity { get; set; }
}