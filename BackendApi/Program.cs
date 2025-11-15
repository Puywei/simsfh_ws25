using Microsoft.EntityFrameworkCore;
using BackendApi.Data.Database;
using BackendApi.Data.Services;
using BackendApi.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
if (builder.Environment.EnvironmentName == "IntegrationTesting")
{
    builder.Services.AddDbContext<MsSqlDbContext>(options =>
        options.UseSqlServer("Data Source=localhost;User ID=sa;Password=eZW6FZ7zswB8Dzy@L9L9cAQBUt*@*jda;Database=SIMSDataTEST;TrustServerCertificate=true"));
}
builder.Services.AddDbContext<MsSqlDbContext>(options =>
    options.UseSqlServer("Data Source=localhost;User ID=sa;Password=eZW6FZ7zswB8Dzy@L9L9cAQBUt*@*jda;Database=SIMSData;TrustServerCertificate=true"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<RedisLogService>();

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MsSqlDbContext>();
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
app.MapLogEndpoints();



app.Run();

// notwendig für Unit Tests
public partial class Program { }
