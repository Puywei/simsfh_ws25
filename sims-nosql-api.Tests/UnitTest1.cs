using sims_nosql_api.Services;
using System.ComponentModel.DataAnnotations;

public class LogEntryValidationTests
{
    [Fact]
    // LogEntry darf nicht leer sein
    public void LogEntry_ShouldFail_WhenMessageIsEmpty()
    {
        // Arrange
        var entry = new LogEntry
        {
            Timestamp = DateTime.UtcNow,
            Message = "",          //darf nicht leer sein
            Severity = 2
        };

        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(entry);

        // Act
        bool isValid = Validator.TryValidateObject(entry, context, validationResults, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(validationResults, v => v.ErrorMessage!.Contains("message darf nicht leer sein"));
    }

    [Fact]
    // Severity muss zwischen 0 und 4 liegen 
    public void LogEntry_ShouldFail_WhenSeverityIsOutOfRange()
    {
        // Arrange
        var entry = new LogEntry
        {
            Timestamp = DateTime.UtcNow,
            Message = "Test message",
            Severity = 10      // <- ungültig
        };

        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(entry);

        // Act
        bool isValid = Validator.TryValidateObject(entry, context, validationResults, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(validationResults, v => v.ErrorMessage!.Contains("severity muss zwischen 0 und 4 liegen"));
    }
}
