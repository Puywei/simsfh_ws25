using BackendApi.Data.Database;
using BackendApi.Data.Model.LogData;

namespace BackendApi.Endpoints;

public static class LoggingEndpoints
{
    public static void MapLogEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder logs = routes.MapGroup("/api/v1/logs");
            
            logs.MapGet("/{id}", async (RedisNoSqlDbContext dbContext, string id) =>
            {
                LogEntry? log = await dbContext.Logs.FirstOrDefaultAsync(l => l.LogId == id);
                return log is not null ? Results.Ok(log) : Results.NotFound();
            })
            .WithName("GetLogById")
            .WithDescription("Returns a log entry by id");

        logs.MapGet("", async (RedisNoSqlDbContext dbContext, int? limit) =>
            {
                IEnumerable<LogEntry> logEntries;

                if (limit.HasValue)
                    logEntries = await dbContext.Logs.TakeAsync(limit.Value);
                else
                    logEntries = await dbContext.Logs.GetAllAsync();

                return Results.Ok(logEntries);
            })
            .WithName("GetLogs")
            .WithDescription("Returns all logs, optional limit for number of entries");

    }

}