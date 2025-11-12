using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace sims_nosql_api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Controller aktivieren
            builder.Services.AddControllers();

            // Swagger aktivieren (einfach, ohne OpenApiInfo)
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Swagger im Entwicklungsmodus aktivieren
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "sims-nosql-api v1");
                c.RoutePrefix = string.Empty; // Startet Swagger direkt unter "/"
            });


            // Standard-Middleware
            app.UseHttpsRedirection();
            app.UseAuthorization();

            // Controller aktivieren
            app.MapControllers();

            // Anwendung starten
            app.Run();
        }
    }
}