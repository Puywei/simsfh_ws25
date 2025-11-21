using BackendApi.Data.Model.Customer;
using BackendApi.Data.Model.Incident;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Data.Database;

public class MsSqlDbContext : DbContext
{
    public DbSet<Incident> Incidents { get; set; }
    public DbSet<IncidentComment> IncidentComments { get; set; }

    public DbSet<Customer> Customers { get; set; }
    
    public MsSqlDbContext(DbContextOptions<MsSqlDbContext> options)
        : base(options)
    {
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