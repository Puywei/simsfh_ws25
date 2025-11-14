using Microsoft.AspNetCore.Mvc;
using sims_nosql_api.Services;

namespace sims_nosql_api.Controller
{
    // Web-APi-Controller; Basis-URL
    [ApiController]
    [Route("api/[controller]")]
    public class RedisController : ControllerBase
    {
        private readonly RedisService _redisService; // Zugriff

        // Konstruktor: Erstellt eine Instanz der RedisService-Klasse
        public RedisController()
        {
            _redisService = new RedisService();
        }

        // POST
        // Speichert einen Wert in Redis unter dem angegebenen Schlüssel
        [HttpPost("set")]
        public async Task<IActionResult> SetValue(string key, string value)
        {
            await _redisService.SetValueAsync(key, value);
            return Ok($"Gespeichert: {key} = {value}");
        }

        // GET
        // Liest den Wert aus Redis, der zum angegebenen Schlüssel gehört
        [HttpGet("get")]
        public async Task<IActionResult> GetValue(string key)
        {
            var value = await _redisService.GetValueAsync(key);

            if (value == null)
                return NotFound($"Kein Wert für den Schlüssel '{key}' gefunden.");

            return Ok($"{key} = {value}");
        }

        // GET
        // Gibt alle gespeicherten Schlüssel und Werte zurück
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var allValues = await _redisService.GetAllValuesAsync();
            return Ok(allValues);
        }

    }
}
