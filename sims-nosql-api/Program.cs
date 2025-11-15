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
            builder.Services.AddControllers()
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.SuppressModelStateInvalidFilter = false;
                });

            // Swagger aktivieren
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SupportNonNullableReferenceTypes();
            });

            // Dependency Injection für Redis
            builder.Services.AddSingleton<RedisConnection>();
            builder.Services.AddSingleton<RedisService>();

            var app = builder.Build();

            // Swagger UI
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "sims-nosql-api v1");
                c.RoutePrefix = string.Empty;
            });

            // HTTPS
            app.UseHttpsRedirection();
            app.UseAuthorization();

            app.MapControllers();
            app.Run();
        }
    }
}
