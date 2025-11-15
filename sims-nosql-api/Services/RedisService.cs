using StackExchange.Redis;
using sims_nosql_api.Database;
using System.Text.Json;

namespace sims_nosql_api.Services
{
    public class RedisService
    {
        private readonly IDatabase _db;
        private readonly IConnectionMultiplexer _connection;

        public RedisService(RedisConnection redisConnection)
        {
            _connection = redisConnection.Connection;
            _db = _connection.GetDatabase();
        }

        // Log speichern
        public async Task SaveLogAsync(LogEntry log)
        {
            string key = $"log:{log.LogId}";
            string json = JsonSerializer.Serialize(log);

            await _db.StringSetAsync(key, json);
        }

        // Einzelnes Log abrufen
        public async Task<LogEntry?> GetLogAsync(int logId)
        {
            string key = $"log:{logId}";
            var value = await _db.StringGetAsync(key);

            if (!value.HasValue)
                return null;

            return JsonSerializer.Deserialize<LogEntry>(value!);
        }

        // Alle Logs abrufen
        public async Task<List<LogEntry>> GetAllLogsAsync()
        {
            var server = _connection.GetServer("redis", 6379);
            var keys = server.Keys(pattern: "log:*");

            var list = new List<LogEntry>();

            foreach (var key in keys)
            {
                var value = await _db.StringGetAsync(key);

                if (value.HasValue)
                {
                    var log = JsonSerializer.Deserialize<LogEntry>(value!);
                    if (log != null)
                        list.Add(log);
                }
            }

            return list;
        }
    }
}
