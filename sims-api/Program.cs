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
        

        builder.Services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                options.Authority = builder.Configuration["Keycloak:Authority"];
                options.Audience = builder.Configuration["Keycloak:Audience"];
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