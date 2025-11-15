using Microsoft.EntityFrameworkCore;
using TestApi.Data.Database;
using TestApi.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddDbContext<ApiDbContext>(options =>
       options.UseSqlServer("Data Source=mssql;User ID=sa;Password=eZW6FZ7zswB8Dzy@L9L9cAQBUt*@*jda;Database=SIMSData;TrustServerCertificate=true"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
    db.Database.EnsureCreated();
}


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapIncidentEndpoints(); 
app.MapCustomerEndpoints();



app.Run();

// notwendig für Unit Tests
public partial class Program { }
