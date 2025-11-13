using Microsoft.EntityFrameworkCore;
using TestApi.Data.Database;
using TestApi.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddDbContext<ApiDbContext>(options =>
       options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Test Daten erstellen fürs testen
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApiDbContext>();
    context.Database.EnsureCreated();
    ApiDbContext.Seed(context);
}

if (app.Environment.IsDevelopment())
{
  //  app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapIncidentEndpoints(); 



app.Run();

// notwendig für Unit Tests
public partial class Program { }
