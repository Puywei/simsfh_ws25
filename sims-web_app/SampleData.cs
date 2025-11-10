using Microsoft.CodeAnalysis.CSharp.Syntax;
using sims_web_app.Data.Database;
using sims_web_app.Data.Models;

namespace sims_web_app;

public class SampleData
{
    public static void SeedData()
    {
        using (SimsDbContext simsDbContext = new SimsDbContext())
        {
            try
            {
                simsDbContext.Database.EnsureCreated();
                simsDbContext.Add(new IncidentResponder() {FirstName = "Max", LastName = "Mustermann", Email = "m.mustermann@muster.at", Id = Guid.NewGuid()});
                simsDbContext.Add(new IncidentResponder() {FirstName = "Peter", LastName = "Mustermann", Email = "p.mustermann@muster.at", Id = Guid.NewGuid()});
                simsDbContext.SaveChanges();
            
                simsDbContext.Add(new Incident()
                {
                    CreatedOn = DateTime.Now,
                    Title = "Test Incident 1",
                    State = IncidentState.Open,
                    Severity = IncidentSeverity.Medium,
                    Description = "Test Desciption 1",
                    Customer = "Geier",
                    IncidentGuid = Guid.NewGuid(),
                    IncidentId = "SIMS-10001", 
                    Responder = simsDbContext.IncidentResponders.FirstOrDefault(r => r.FirstName == "Peter")
                    
                });
                simsDbContext.Add(new Incident()
                {
                    CreatedOn = DateTime.Now,
                    Title = "Test Incident 2",
                    State = IncidentState.Open,
                    Severity = IncidentSeverity.Medium,
                    Description = "Test Desciption 2",
                    Customer = "Piller",
                    IncidentGuid = Guid.NewGuid(),
                    IncidentId = "SIMS-10002",
                    Responder = simsDbContext.IncidentResponders.FirstOrDefault(r => r.FirstName == "Max")
                });
                simsDbContext.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine("Cant create Database or Seed Data");
            }
            
            
        }
    }
   
}