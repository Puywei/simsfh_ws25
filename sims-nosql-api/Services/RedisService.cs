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


    /** 
        VULNERABILITY:  [ Log Injection ]
        DESCRIPTION:    [ The log message content is accepted without additional content validation.
                        An attacker could inject control characters or crafted log messages
                        to manipulate log output, mislead monitoring systems or falsify log analysis. ]
        MITIGATION:     [ Validate and sanitize the log message content before storing it.
                        Reject messages containing control characters or unexpected formatting
                        to ensure log integrity and prevent manipulation. ]
        


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
        } **/
        
        
        // NEW CODE including message-check
        public async Task<int> SaveLogAsync(LogEntry log)
        {
            // Log-Injection Schutz: Message-Inhalt prüfen
            if (log.Message.Contains("\n") || log.Message.Contains("\r"))
            {
                throw new ArgumentException("Log message contains invalid control characters.");
            }

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

        /** VULNERABILITY:  [ Serialization Security ]
            DESCRIPTION:    [ Data from Redis is deserialized directly, without safe error handling and without content validation. 
                            Maniuplated data can cause exeptions, invalid objects or unstable states. ]
        
            MITIGATION:     [ Protect deserialization with try-catch and, after deserialization, verify that the result contains only allowed values
                            and matches the expected structure. ]
        

        public async Task<LogEntry?> GetLogAsync(int logId)
        {
            string key = $"log:{logId}";
            var value = await _db.StringGetAsync(key);

            if (!value.HasValue)
                return null;

            return JsonSerializer.Deserialize<LogEntry>(value!);
        } 
        **/

        public async Task<LogEntry?> GetLogAsync(int logId)
        {
            string key = $"log:{logId}";
            var value = await _db.StringGetAsync(key);

            if (!value.HasValue)
                return null;

            try
            {
                var log = JsonSerializer.Deserialize<LogEntry>(value!);

                if (log == null)
                    return null;

                if (log.LogId < 1)
                    return null;

                if (string.IsNullOrWhiteSpace(log.Message))
                    return null;

                if (log.Message.Length > 500)
                    return null;

                if (log.Severity < 0 || log.Severity > 4)
                    return null;

                return log;
            }
            catch
            {
                return null;
            }
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

                if (!value.HasValue)
                    continue;

                string str = value.ToString().Trim();



                /** VULNERABILITY:  [ Serialization Security ]
                    DESCRIPTION:    [ Data from Redis is deserialized directly, without safe error handling and without content validation. 
                                    Maniuplated data can cause exeptions, invalid objects or unstable states. ]
        
                    MITIGATION:     [ Protect deserialization with try-catch and, after deserialization, verify that the result contains only allowed values
                                    and matches the expected structure. ]
                

                Nur gültige JSON-Objekte akzeptieren (beginnen mit '{')
                if (!str.StartsWith("{"))
                    continue;              
                **/


                // NEU: Leere oder ungültige Inhalte direkt überspringen

                if (string.IsNullOrWhiteSpace(str))
                    continue;

                try
                {
                    var log = JsonSerializer.Deserialize<LogEntry>(str);

                    if (log == null)
                        continue;

                    if (log.LogId < 1)
                        continue;

                    if (string.IsNullOrWhiteSpace(log.Message))
                        continue;

                    if (log.Message.Length > 500)
                        continue;

                    if (log.Severity < 0 || log.Severity > 4)
                        continue;

                    list.Add(log);
                }
                catch
                {
                    // Fehlerhafte oder manipulierte Daten ignorieren
                    continue;
                }
            }

            return list;
        }
    }
}
