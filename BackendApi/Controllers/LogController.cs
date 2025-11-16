using BackendApi.Data.Model.LogData;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Text.Json;

namespace BackendApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class LogsController : ControllerBase
    {
        private readonly IDatabase _db;

        public LogsController(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LogEntry>>> GetLogs([FromQuery] int count = 50)
        {
            // LRANGE list: start=0, end=count-1
            var redisValues = await _db.ListRangeAsync("logs", 0, count - 1);

            var logs = redisValues
                .Select(rv => JsonSerializer.Deserialize<LogEntry>(rv)!)
                .ToList();

            return Ok(logs);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<LogEntry>> GetLogById(string id)
        {
            var redisValues = await _db.ListRangeAsync("logs", 0, -1);
            foreach (var rv in redisValues)
            {
                var log = JsonSerializer.Deserialize<LogEntry>(rv);
                if (log != null && log.Id == id)
                    return Ok(log);
            }

            return NotFound();
        }
        
    }
}
