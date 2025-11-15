using StackExchange.Redis;
using Microsoft.Extensions.Configuration;

namespace sims_nosql_api.Database
{
    public class RedisConnection
    {
        private readonly Lazy<IConnectionMultiplexer> _lazyConnection;

        public RedisConnection(IConfiguration configuration)
        {
            var connectionString = configuration["Redis:ConnectionString"];

            _lazyConnection = new Lazy<IConnectionMultiplexer>(() =>
            {
                return ConnectionMultiplexer.Connect(connectionString);
            });
        }

        public IConnectionMultiplexer Connection => _lazyConnection.Value;
    }
}
