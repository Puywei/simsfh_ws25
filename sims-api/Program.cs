using System;
using Microsoft.EntityFrameworkCore;
using sims.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using sims.Helpers;
using sims.Services;

namespace sims;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

      
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "sims-api", Version = "v1" });
            
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter 'Bearer' [space] and then your token.\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6...\""
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
            });
        });

    
        builder.Services.AddDbContext<UserDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
        
        
    
        var jwtKey = builder.Configuration["Jwt:Key"];
        var jwtIssuer = builder.Configuration["Jwt:Issuer"];

        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtIssuer,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                };
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<IEventLogger>();
                        await logger.LogEventAsync(
                            $"Authorization challenge: Invalid or missing token for {context.HttpContext.Request.Path}",
                            severity: 2
                        );
                    },
                    
                    OnForbidden = async context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<IEventLogger>();
                        var actingUser = UserContextHelper.GetActingUserInfo(context.HttpContext.User);

                        await logger.LogEventAsync(
                            $"Authorization failed: User '{actingUser.Email}' (Role='{actingUser.Role}') attempted to access {context.HttpContext.Request.Path} without required role.",
                            severity: 3
                        );
                    },
                    OnMessageReceived = async context =>
                    {
                        var db = context.HttpContext.RequestServices.GetRequiredService<UserDbContext>();
                        var logger = context.HttpContext.RequestServices.GetRequiredService<IEventLogger>();
                        var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                        if (!string.IsNullOrEmpty(token))
                        {
                            var isBlacklisted = await db.BlacklistedTokens.AnyAsync(b => b.Token == token);
                            if (isBlacklisted)
                            {
                                await logger.LogEventAsync(
                                    $"Blacklisted token used for {context.HttpContext.Request.Path}",
                                    severity: 3
                                );
                                context.Fail("Token is blacklisted.");
                            }
                        }
                    }
                };
            });
        builder.Services.AddHttpClient("EventLogger", client =>
        {
            client.BaseAddress = new Uri("http://sims-nosql-api:8080/");
        });
        builder.Services.AddScoped<IEventLogger, EventLogger>();

        var app = builder.Build();
        
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
            db.Database.Migrate();
            db.Seed();
        }
        
        
        
        
        // For Swagger (only during the development....I guess
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}