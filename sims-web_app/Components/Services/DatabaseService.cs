using sims_web_app.Data.Database;
using sims_web_app.Data.Models;

namespace sims_web_app.Components.Services;

public class DatabaseService
{
    public static List<Data.Models.Incident> GetIncidents()
    {
        List<Data.Models.Incident> incidents = new List<Data.Models.Incident>();
        using (SimsDbContext simsDbContext = new SimsDbContext())
        {
            foreach (Data.Models.Incident incident in simsDbContext.Incidents)
            {
                incidents.Add(incident);
            }
        }
        return incidents;
    }
}