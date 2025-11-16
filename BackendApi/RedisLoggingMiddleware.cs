using BackendApi.Data.Model.LogData;
using BackendApi.Data.Services;

namespace BackendApi;

public class RedisLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RedisLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IRedisLogService logService)
    {
        LogEntry log = new LogEntry
        {
            Path = context.Request.Path,
            Method = context.Request.Method,
            User = context.User?.Identity?.Name ?? "anonymous",
            Timestamp = DateTime.UtcNow
        };

        await logService.LogRequestAsync(log);

        await _next(context);
    }
}
