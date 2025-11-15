using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using TestApi.Data.Model.Enum;
using TestApi.Data.Model.Incident;

namespace TestApi.Data.Database;

public class ApiDbContext : DbContext
{
    private IConfiguration Configuration { get; }
    
    public ApiDbContext(IConfiguration configuration)
    {
        Configuration = configuration;
    }
    
    public DbSet<Incident> Incidents { get; set; }
    public DbSet<IncidentComment> IncidentComments { get; set; }
    
    public DbSet<Customer> Customers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        Console.WriteLine(Configuration.GetConnectionString("DefaultConnection"));
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Data Source=mssql;User ID=sa;Password=eZW6FZ7zswB8Dzy@L9L9cAQBUt*@*jda;Database=SIMSData;TrustServerCertificate=true");

            Console.WriteLine("Test 2");
           // optionsBuilder.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
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