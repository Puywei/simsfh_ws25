using BackendApi;
using BackendApi.Data.Database;
using BackendApi.Data.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
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

builder.Services.AddDbContext<MsSqlDbContext>(options =>
    options.UseSqlServer(Environment.GetEnvironmentVariable("MSSQL_CONNECTION")));

builder.Services.AddSingleton<IConnectionMultiplexer>(provider =>
{
    string? redisConnectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION");
    return ConnectionMultiplexer.Connect(redisConnectionString);
});

builder.Services.AddScoped<IRedisLogService, RedisLogService>();

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MsSqlDbContext>();
    if (!db.Database.CanConnect())
    {
        db.Database.EnsureCreated();
    }
}


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<RedisLoggingMiddleware>();
app.MapControllers();


app.Run();

// notwendig für Unit Tests
public partial class Program
{
}