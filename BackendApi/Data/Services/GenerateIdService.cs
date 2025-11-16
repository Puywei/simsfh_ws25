using BackendApi.Data.Database;

namespace BackendApi.Data.Services;

public static class GenerateIdService
{
    public static string IncidentId(MsSqlDbContext dbContext)
    {
        var lastIncident = dbContext.Incidents.OrderByDescending(inc => inc.Id).FirstOrDefault();

        var newIncidentNr = 1;

        if (lastIncident != null)
        {
            var lastId = lastIncident.Id!;
            if (int.TryParse(lastId.Substring(4), out var lastIncidentNr)) newIncidentNr = lastIncidentNr + 1;
        }

        return $"INC-{newIncidentNr:D4}";
    }

    public static string CustomerId(MsSqlDbContext dbContext)
    {
        var lastCustomer = dbContext.Customers.OrderByDescending(c => c.Id).FirstOrDefault();

        var newCustomerId = 1;

        if (lastCustomer != null)
        {
            var lastId = lastCustomer.Id!;
            if (int.TryParse(lastId.Substring(2), out var lastCustomerNr)) newCustomerId = lastCustomerNr + 1;
        }

        return $"C-{newCustomerId:D4}";
    }
}