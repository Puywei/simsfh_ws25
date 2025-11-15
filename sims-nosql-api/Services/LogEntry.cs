using System.ComponentModel.DataAnnotations;

namespace sims_nosql_api.Services
{
    public class LogEntry
    {
        // logId muss > 0 sein
        [Range(1, int.MaxValue, ErrorMessage = "logId muss größer als 0 sein.")]
        public int LogId { get; set; }

        // timestamp muss gültig & vorhanden sein
        [Required(ErrorMessage = "timestamp ist erforderlich.")]
        public DateTime Timestamp { get; set; }

        // message darf nicht leer sein, max 500 Zeichen
        [Required(ErrorMessage = "message darf nicht leer sein.")]
        [MaxLength(500, ErrorMessage = "message darf maximal 500 Zeichen lang sein.")]
        public string Message { get; set; } = string.Empty;

        // severity muss zwischen 0 und 4 liegen
        [Range(0, 4, ErrorMessage = "severity muss zwischen 0 und 4 liegen.")]
        public int Severity { get; set; }
    }
}
