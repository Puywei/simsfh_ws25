using StackExchange.Redis;
using System.Text.Json;
using BackendApi.Data.Database;
using BackendApi.Data.Model.LogData;


namespace BackendApi.Data.Services;

public class RedisLogService
{
    private readonly RedisNoSqlDbContext _dbContext;

    public RedisLogService(RedisNoSqlDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<LogEntry> LogAsync(string message)
    {
        var log = new LogEntry
        {
            LogId = Guid.NewGuid().ToString(),
            Message = message,
            Timestamp = DateTime.UtcNow
        };

        await _dbContext.Logs.AddAsync(log.LogId, log);
        return log;
    }
}
