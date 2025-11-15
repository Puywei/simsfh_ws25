using System.ComponentModel.DataAnnotations;

namespace BackendApi.Data.Model.LogData;

public class LogEntry
{
    [Range(1, int.MaxValue, ErrorMessage = "logId muss größer als 0 sein.")]
    public string LogId { get; set; }
    
    [Required(ErrorMessage = "timestamp ist erforderlich.")]
    public DateTime Timestamp { get; set; }
    
    [Required(ErrorMessage = "message darf nicht leer sein.")]
    [MaxLength(500, ErrorMessage = "message darf maximal 500 Zeichen lang sein.")]
    public string Message { get; set; } = string.Empty;
    
    [Range(0, 4, ErrorMessage = "severity muss zwischen 0 und 4 liegen.")]
    public int Severity { get; set; }
}