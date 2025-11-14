using StackExchange.Redis;
using sims_nosql_api.Database;

namespace sims_nosql_api.Services
{
    public class RedisService
    {
        // Interface zur Datenbank 
        private readonly IDatabase _db;

        public RedisService()
        {
            // Verwendet die bestehende Redis-Verbindung aus deiner RedisConnection-Klasse
            _db = RedisConnection.Connection.GetDatabase();
        }

        // speichert einen Wert // PRÜFUNG AUF NULL und LÄNGE OFFEN
        public async Task SetValueAsync(string key, string value)
        {
            await _db.StringSetAsync(key, value);
        }

        // Wert aus Redis lesen
        public async Task<string?> GetValueAsync(string key)
        {
            var value = await _db.StringGetAsync(key);
            return value.HasValue ? value.ToString() : null;
        }
    

    // alle Daten gesamt abrufen
public async Task<Dictionary<string, string>> GetAllValuesAsync()
        {
            var server = RedisConnection.Connection.GetServer("redis", 6379);
            var keys = server.Keys();

            var result = new Dictionary<string, string>();

            foreach (var key in keys)
            {
                string value = await _db.StringGetAsync(key);
                result.Add(key, value);
            }

            return result;
        }
    } 
}
    

