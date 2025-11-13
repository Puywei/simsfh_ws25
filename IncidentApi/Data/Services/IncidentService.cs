using TestApi.Data.Database;
using TestApi.Data.Model.Incident;

namespace TestApi.Data.Services;

public class IncidentService
{
    public static string GenerateIncidentId(ApiDbContext dbContext)
    {
        Incident lastIncident = dbContext.Incidents.OrderByDescending(inc => inc.IncidentId).FirstOrDefault();

        int newIncidentNr = 1; 
        
        if (lastIncident != null)
        {
            string lastId = lastIncident.IncidentId!; // z.B. "INC-0042"
            if (int.TryParse(lastId.Substring(4), out int lastIncidentNr))
            {
                newIncidentNr = lastIncidentNr +1;
            }
        }

        return $"INC-{newIncidentNr:D4}";
    }
}