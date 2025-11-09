using Microsoft.EntityFrameworkCore;
using sims_web_app.Components.Models.Entities;

namespace sims_web_app.Components.Data;

public class AppDbContext : DbContext
{
    private readonly IConfiguration _configuration;

    public AppDbContext(IConfiguration configuration)
    {
        this._configuration = configuration;
    }
    
    public DbSet<UserAccount> UserAccounts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(this._configuration.GetConnectionString("DefaultConnection"));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<UserAccount>().HasData(
            new UserAccount
            {
                Id = 1,
                Username = "admin",
                Password = "admin",
                Email = "admin@app.at",
                Role = "Admin"
            },
            new UserAccount
            {
                Id = 2,
                Username = "user",
                Password = "user",
                Email = "user@app.at",
                Role = "User"
            });
    }
}