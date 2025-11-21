using BackendApi;
using BackendApi.Data.Database;
using BackendApi.Data.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using sims.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SIMS Backend API", Version = "v1" });
    c.EnableAnnotations();
});


var isTesting = Environment.GetEnvironmentVariable("TESTING") == "true";

builder.Services.AddDbContext<MsSqlDbContext>(options =>
{
    if (isTesting)
    {
        options.UseInMemoryDatabase("TestDb");
    }
    else
    {
        options.UseSqlServer(Environment.GetEnvironmentVariable("MSSQL_CONNECTION"));
    }
});

if (!isTesting)
{
    builder.Services.AddHttpClient("EventLogger", client =>
    {
        client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("REDIS_CONNECTION"));
    });
    builder.Services.AddScoped<IEventLogger, EventLogger>();
}




var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MsSqlDbContext>();
    if (!isTesting)
    {
        db.Database.Migrate();
    }
}


    app.UseSwagger();
    app.UseSwaggerUI();

app.MapControllers();


app.Run();

// notwendig für Unit Tests
public partial class Program
{
}