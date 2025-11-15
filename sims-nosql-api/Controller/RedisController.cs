using Microsoft.AspNetCore.Mvc;
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

        // POST: /api/Redis/log -- Log speichern
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
