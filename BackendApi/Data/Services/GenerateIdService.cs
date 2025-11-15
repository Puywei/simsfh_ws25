using BackendApi.Data.Database;
using BackendApi.Data.Model.Incident;

namespace BackendApi.Data.Services;

public static class GenerateIdService
{
    public static string IncidentId(ApiDbContext dbContext)
    {
        Incident? lastIncident = dbContext.Incidents.OrderByDescending(inc => inc.Id).FirstOrDefault();

        int newIncidentNr = 1; 
        
        if (lastIncident != null)
        {
            string lastId = lastIncident.Id!;
            if (int.TryParse(lastId.Substring(4), out int lastIncidentNr))
            {
                newIncidentNr = lastIncidentNr +1;
            }
        }

        return $"INC-{newIncidentNr:D4}";
    }
    
    public static string CustomerId(ApiDbContext dbContext)
    {
        Customer? lastCustomer = dbContext.Customers.OrderByDescending(c => c.Id).FirstOrDefault();

        int newCustomerId = 1; 
        
        if (lastCustomer != null)
        {
            string lastId = lastCustomer.Id!;
            if (int.TryParse(lastId.Substring(2), out int lastCustomerNr))
            {
                newCustomerId = lastCustomerNr +1;
            }
        }

        return $"C-{newCustomerId:D4}";
    }
}