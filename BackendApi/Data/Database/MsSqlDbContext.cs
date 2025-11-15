using BackendApi.Data.Model.Customer;
using BackendApi.Data.Model.Incident;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Data.Database;

public class MsSqlDbContext : DbContext
{
    private IConfiguration Configuration { get; }
    
    public MsSqlDbContext(IConfiguration configuration)
    {
        Configuration = configuration;
    }
    
    public DbSet<Incident> Incidents { get; set; }
    public DbSet<IncidentComment> IncidentComments { get; set; }
    
    public DbSet<Customer> Customers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Prüfen ob Test-Umgebung
            var isTest = Environment.GetEnvironmentVariable("TESTING") == "true";

            if (isTest)
            {
                optionsBuilder.UseInMemoryDatabase("TestDb");
            }
            else
            {
                optionsBuilder.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection"));
            }
        }
        
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Incident>()
            .HasMany(i => i.Comments)
            .WithOne(c => c.Incident)
            .HasForeignKey(c => c.IncidentId)
            .OnDelete(DeleteBehavior.Cascade); 
    }
    
}