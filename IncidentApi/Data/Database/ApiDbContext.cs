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

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
            optionsBuilder.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
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
    
public static void Seed(ApiDbContext context)
{
    if (context.Incidents.Any())
        return;

    var random = new Random();
    var authors = new[] { "Alice", "Bob", "Charlie", "Diana", "Eve" };
    var assignees = new[] { "Max", "Julia", "Tom", "Laura", "Nina" };
    var summaries = new[]
    {
        "System outage detected",
        "Login issue reported",
        "Data synchronization failed",
        "Unexpected API response",
        "Memory leak in service"
    };

    for (int i = 0; i < 20; i++)
    {
        var incidentId = $"INC-{1000 + i}";
        var incident = new Incident
        {
            IncidentId = incidentId,
            Summary = summaries[random.Next(summaries.Length)],
            Description = $"Test description for incident #{i + 1}",
            CreateDate = DateTime.Now.AddDays(-random.Next(30)),
            UpdateDate = DateTime.Now,
            Status = (IncidentStatus)random.Next(Enum.GetValues(typeof(IncidentStatus)).Length),
            IncidentType = IncidentType.SecurityIncident,
            AssignedPerson = assignees[random.Next(assignees.Length)],
            Severity = (IncidentSeverity)random.Next(Enum.GetValues(typeof(IncidentSeverity)).Length),
            Author = authors[random.Next(authors.Length)],
            IncidentUUid = Guid.NewGuid()
        };

        context.Incidents.Add(incident);
        context.SaveChanges(); 
        
        int commentCount = random.Next(1, 5);
        for (int j = 0; j < commentCount; j++)
        {
            var comment = new IncidentComment
            {
                CommentId = Guid.NewGuid(),
                Incident = incident,  
                UserId = Guid.NewGuid(),
                UserName = authors[random.Next(authors.Length)],
                CreateDate = DateTime.Now.AddMinutes(-random.Next(1000)),
                Comment = $"Comment #{j + 1} for incident {incidentId}",
                CommentOrder = j + 1
            };

            context.IncidentComments.Add(comment);
        }

        context.SaveChanges(); 
    }
}



}