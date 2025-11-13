using StackExchange.Redis;

namespace sims_nosql_api.Database
{
    public static class RedisConnection
    {
        private static Lazy<IConnectionMultiplexer> lazyConnection = new Lazy<IConnectionMultiplexer>(() =>
        {
            // Verbindung zur Redis-Datenbank im Docker-Netzwerk
            return ConnectionMultiplexer.Connect("redis:6379");
        });

        public static IConnectionMultiplexer Connection => lazyConnection.Value;
    }
}

