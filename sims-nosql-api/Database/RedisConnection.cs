using StackExchange.Redis; //Bibliothek für die Verbindung zu Redis

namespace sims_nosql_api.Database
{
    public static class RedisConnection
    {
        // lazy: Verbindung wird erst hergestellt, wenn sie gebraucht wird; nicht direkt beim Start
        // multiplexer: Verbindungsverwaltung zur DB um set, get Befehle zu senden
        private static Lazy<IConnectionMultiplexer> lazyConnection = new Lazy<IConnectionMultiplexer>(() =>
        {
            // Verbindung zur Redis-Datenbank im Docker-Netzwerk
            return ConnectionMultiplexer.Connect("redis:6379");
        });

        // bestehende Verbindung erhalten, damit andere Teile (services, controller) über redisconnection zugreifen können
        public static IConnectionMultiplexer Connection => lazyConnection.Value;
    }
}

// lädt die Redis Client Bibliothek
// baut eine Verbindung auf
// stellt die Verbindung zu Redis her
// gibt die Verbindung an andere Klassen weiter