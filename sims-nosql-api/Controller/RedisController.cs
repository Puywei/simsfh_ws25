using Microsoft.AspNetCore.Mvc;
using sims_nosql_api.Services;

namespace sims_nosql_api.Controller
{
    // Diese Klasse stellt eine Web-API bereit, um Daten in Redis zu speichern und zu lesen
    [ApiController]
    [Route("api/[controller]")]
    public class RedisController : ControllerBase
    {
        private readonly RedisService _redisService;

        // Konstruktor: Erstellt eine Instanz der RedisService-Klasse
        public RedisController()
        {
            _redisService = new RedisService();
        }

        // POST: /api/redis/set?key=beispiel&value=123
        // Speichert einen Wert in Redis unter dem angegebenen Schlüssel
        [HttpPost("set")]
        public async Task<IActionResult> SetValue(string key, string value)
        {
            await _redisService.SetValueAsync(key, value);
            return Ok($"Gespeichert: {key} = {value}");
        }

        // GET: /api/redis/get?key=beispiel
        // Liest den Wert aus Redis, der zum angegebenen Schlüssel gehört
        [HttpGet("get")]
        public async Task<IActionResult> GetValue(string key)
        {
            var value = await _redisService.GetValueAsync(key);

            if (value == null)
                return NotFound($"Kein Wert für den Schlüssel '{key}' gefunden.");

            return Ok($"{key} = {value}");
        }
    }
}
