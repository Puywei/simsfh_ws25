using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using sims_nosql_api.Database;
using sims_nosql_api.Services;

namespace sims_nosql_api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // appsettings.json laden
            builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            // Controller aktivieren
            builder.Services.AddControllers();

            // Swagger aktivieren
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Dependency Injection registrieren
            builder.Services.AddSingleton<RedisConnection>();
            builder.Services.AddSingleton<RedisService>();

            var app = builder.Build();

            // Swagger im Browser anzeigen
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "sims-nosql-api v1");
                    c.RoutePrefix = string.Empty;
                });
            }
            else
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "sims-nosql-api v1");
                    c.RoutePrefix = string.Empty;
                });
            }

            // HTTPS-Weiterleitung
            app.UseHttpsRedirection();

            // Authorization Middleware (optional)
            app.UseAuthorization();

            // Controller registrieren
            app.MapControllers();

            // Anwendung starten
            app.Run();
        }
    }
}
