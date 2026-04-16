using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using sims_nosql_api.Services;

namespace sims_nosql_api.Controller
{
    // API-Endpunkte für das Loggen in Redis
    [ApiController]
    [Route("api/[controller]")]
    public class RedisController : ControllerBase
    {
        private readonly RedisService _redisService;

        // Dependency Injection: RedisService wird von außen bereitgestellt
        public RedisController(RedisService redisService)
        {
            _redisService = redisService;
        }

        /**
        VULNERABILITY 1:  [ Overposting / Mass Assignment ]
        DESCRIPTION:      [ The API directly accepts the internal LogEntry model in the POST request.
                          A client can send additional or sensitive properties (e.g. logId),
                          which should not be controlled by external input.
                          This can lead to manipulated data or unexpected behavior. ]

       MITIGATION:       [ Use a dedicated Data Transfer Object (DTO) with only the allowed input fields.
                         Map the DTO explicitly to the internal model before saving the data. ]

       IMPLEMENTATION NOTE: [ DTO was not implementet. For implementation we would use a dedicated DTO containing only the allowed fields
                            for client input & update the POST part to accept the DTO and manually map its values to the internal 
                            LogEntry model before saving.]

        VULNERABILITY 2:  [ Missing Rate Limiting ]
        DESCRIPTION:      [ The API does not limit how often clients can call the log endpoint.
                          An attacker can send a large number of requests in a short time and flood the system with log entries. ]

        MITIGATION:       [ Introduce rate limiting to restrict the number of requests per client within a defined time window. ]

**/

        // POST: /api/Redis/log -- Log speichern

        [EnableRateLimiting("logLimiter")]
        [HttpPost("log")]
        public async Task<IActionResult> SaveLog([FromBody] LogEntry log)
        {
            // automatische Validierung prüfen
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _redisService.SaveLogAsync(log);
            return Ok($"Log-Eintrag {log.LogId} gespeichert.");
        }

    

        // GET: /api/Redis/log/{id} -- Einzelnes Log abrufen
        [HttpGet("log/{id}")]
        public async Task<IActionResult> GetLog(int id)
        {
            var log = await _redisService.GetLogAsync(id);

            if (log == null)
                return NotFound($"Kein Log mit der ID {id} gefunden.");

            return Ok(log);
        }

        // GET: /api/Redis/logs -- Alle Logs abrufen
        [HttpGet("logs")]
        public async Task<IActionResult> GetAllLogs()
        {
            var logs = await _redisService.GetAllLogsAsync();
            return Ok(logs);
        }
    }
}
