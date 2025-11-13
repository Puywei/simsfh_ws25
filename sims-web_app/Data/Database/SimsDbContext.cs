using Microsoft.EntityFrameworkCore;
using sims_web_app.Data.Models;

namespace sims_web_app.Data.Database;

public class SimsDbContext : DbContext
{
    
    public DbSet<IncidentResponder>  IncidentResponders { get; set; }
    public DbSet<Incident>  Incidents { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(
                @"Data Source=localhost;User ID=sa;Password=mbsa3328!;Database=SIMSData;TrustServerCertificate=true;");
    }
}