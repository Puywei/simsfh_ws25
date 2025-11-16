using BackendApi.Data.Database;
using BackendApi.Data.Model.LogData;
using StackExchange.Redis;
using System.Text.Json;


namespace BackendApi.Data.Services;

public interface IRedisLogService
{
    Task LogRequestAsync(LogEntry entry);
}

public class RedisLogService : IRedisLogService
{
    private readonly IDatabase _db;

    public RedisLogService(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task LogRequestAsync(LogEntry entry)
    {
        var json = JsonSerializer.Serialize(entry);
        await _db.ListLeftPushAsync("logs", json);
    }
}
