using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace sims_nosql_api
{
    public class Program
    {
        // Startskript Web-API
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args); // Start WebApp

            // 
            builder.Services.AddControllers();

            // Swagger Testseite einschalten
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Swagger im Browser anzeigen
           
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "sims-nosql-api v1");
                c.RoutePrefix = string.Empty; // Swagger direkt unter http.. aufrufen
            });



            // https vorbereitung
            app.UseHttpsRedirection();
            app.UseAuthorization();

            // Controller aktivieren
            app.MapControllers();

            // Anwendung starten
            app.Run();
        }
    }
}