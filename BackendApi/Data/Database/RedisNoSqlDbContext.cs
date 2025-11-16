using BackendApi.Data.Model.LogData;
using StackExchange.Redis;

namespace BackendApi.Data.Database;

public class RedisNoSqlDbContext
{
    public RedisNoSqlDbContext()
    {
        ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable("REDIS_CONNECTION"));
        try
        {
            IDatabase db = redis.GetDatabase();
            Logs = new RedisNoSqlDbContextDbSet<LogEntry>(db);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

    }

    public RedisNoSqlDbContextDbSet<LogEntry> Logs { get; }
}