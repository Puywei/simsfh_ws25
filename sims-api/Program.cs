using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace sims;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure services
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        string serverHost = "localhost";
        int  serverPort = 8080;
        

        builder.Services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                options.Authority = $"http://{serverHost}:{serverPort}/realms/IncidentSystem";
                options.Audience = "user-api";
                options.RequireHttpsMetadata = false;
            });

        builder.Services.AddAuthorization();

        var app = builder.Build();

        // Configure middleware
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();   // <-- important: add before Authorization
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}