namespace sims_nosql_api.Services
{
    // wie ein Log-Eintrag in Redis gespeichert wird
    public class LogEntry
    {
        public int LogId { get; set; }              // ID für den Logeintrag
        public DateTime Timestamp { get; set; }     // Timestamp Log
        public string Message { get; set; } = "";   // Message
        public int Severity { get; set; }           // Severity
    }
}
