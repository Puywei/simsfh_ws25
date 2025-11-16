using BackendApi.Data.Model.Customer;
using BackendApi.Data.Model.Incident;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Data.Database;

public class MsSqlDbContext : DbContext
{
    public MsSqlDbContext(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    private IConfiguration Configuration { get; }

    public DbSet<Incident> Incidents { get; set; }
    public DbSet<IncidentComment> IncidentComments { get; set; }

    public DbSet<Customer> Customers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var isTest = Environment.GetEnvironmentVariable("TESTING") == "true";

            if (isTest)
                optionsBuilder.UseInMemoryDatabase("TestDb");
            else
                optionsBuilder.UseSqlServer((Environment.GetEnvironmentVariable("MSSQL_CONNECTION")));
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