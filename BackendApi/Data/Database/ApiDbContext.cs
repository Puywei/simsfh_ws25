using BackendApi.Data.Model.Incident;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Data.Database;

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