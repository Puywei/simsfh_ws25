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

        // neu hinzugefügt:  Log-ID generieren (INCR)
        private async Task<int> GenerateNewLogIdAsync()
        {
            return (int)await _db.StringIncrementAsync("log:id_counter");
        }

        // Log speichern - Client logId wird ignoriert falls der client eine sendet
        public async Task<int> SaveLogAsync(LogEntry log)
        {
            // Schritt 1: immer eine neue ID erzeugen
            int newId = await GenerateNewLogIdAsync();
            log.LogId = newId;

            // Schritt 2: Log speichern
            string key = $"log:{newId}";
            string json = JsonSerializer.Serialize(log);

            await _db.StringSetAsync(key, json);

            // Schritt 3: neue ID zurückgeben
            return newId;
        }

        //  Log abrufen
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
                // Counter ignorieren
                if (key.ToString() == "log:id_counter")
                    continue;

                var value = await _db.StringGetAsync(key);

                if (value.HasValue)
                {
                    try
                    {
                        var log = JsonSerializer.Deserialize<LogEntry>(value!);
                        if (log != null)
                            list.Add(log);
                    }
                    catch
                    {
                        // Falls irgendein anderer “komischer” Key existiert
                        // wird übersprungen statt Fehler zu werfen
                        continue;
                    }
                }
            }

            return list;
        }
    }
}
