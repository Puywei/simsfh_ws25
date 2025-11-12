using StackExchange.Redis;
using sims_nosql_api.Database;

namespace sims_nosql_api.Services
{
    public class RedisService
    {
        private readonly IDatabase _db;

        public RedisService()
        {
            // Verwendet die bestehende Redis-Verbindung aus deiner RedisConnection-Klasse
            _db = RedisConnection.Connection.GetDatabase();
        }

        // Einen Wert speichern
        public async Task SetValueAsync(string key, string value)
        {
            await _db.StringSetAsync(key, value);
        }

        // Einen Wert lesen
        public async Task<string?> GetValueAsync(string key)
        {
            var value = await _db.StringGetAsync(key);
            return value.HasValue ? value.ToString() : null;
        }
    }
}
