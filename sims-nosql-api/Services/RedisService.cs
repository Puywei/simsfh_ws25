using StackExchange.Redis;
using sims_nosql_api.Database;
using System.Text.Json;
using sims_nosql_api.Services;

namespace sims_nosql_api.Services
{
    public class RedisService
    {
        private readonly IDatabase _db;

        public RedisService()
        {
            _db = RedisConnection.Connection.GetDatabase();
        }

        // Log-Eintrag speichern
        public async Task SaveLogAsync(LogEntry log)
        {
            string key = $"log:{log.LogId}";
            string jsonData = JsonSerializer.Serialize(log);

            await _db.StringSetAsync(key, jsonData);
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
            var server = RedisConnection.Connection.GetServer("redis", 6379);
            var keys = server.Keys(pattern: "log:*");

            var result = new List<LogEntry>();

            foreach (var key in keys)
            {
                var value = await _db.StringGetAsync(key);

                if (value.HasValue)
                {
                    var log = JsonSerializer.Deserialize<LogEntry>(value!);
                    if (log != null)
                        result.Add(log);
                }
            }

            return result;
        }
    }
}
