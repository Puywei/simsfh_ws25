using BackendApi.Data.Model.LogData;
using StackExchange.Redis;
using Microsoft.Extensions.Configuration;

namespace BackendApi.Data.Database;
public class RedisNoSqlDbContext
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;

    public RedisNoSqlDbContext(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _db = redis.GetDatabase();
        Logs = new RedisNoSqlDbContextDbSet<LogEntry>(_db);
    }

    public RedisNoSqlDbContextDbSet<LogEntry> Logs { get; }
}
