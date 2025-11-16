using System.Text.Json;
using BackendApi.Data.Model.LogData;
using StackExchange.Redis;

namespace BackendApi.Data.Database;

public class RedisNoSqlDbContextDbSet<T> where T : class
{
    private readonly IDatabase _db;
    private readonly string _prefix;

    public RedisNoSqlDbContextDbSet(IDatabase db)
    {
        _db = db;
        _prefix = typeof(T).Name + ":";
    }

    private string Key(string id)
    {
        return _prefix + id;
    }

    public async Task<T?> FirstOrDefaultAsync(Func<T, bool> predicate)
    {
        var all = await GetAllAsync();
        return all.FirstOrDefault(predicate);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        var server = _db.Multiplexer.GetServer(_db.Multiplexer.GetEndPoints().First());
        var keys = server.Keys(pattern: _prefix + "*");

        var list = new List<T>();
        foreach (var key in keys)
        {
            var value = await _db.StringGetAsync(key);
            if (!value.IsNullOrEmpty)
                list.Add(JsonSerializer.Deserialize<T>(value!)!);
        }

        return list.OrderByDescending(e => (e as LogEntry)?.Timestamp); // nur für Logs relevant
    }

    public async Task<IEnumerable<T>> TakeAsync(int count)
    {
        var all = (await GetAllAsync()).Take(count);
        return all;
    }

    public async Task AddAsync(string id, T entity)
    {
        var json = JsonSerializer.Serialize(entity);
        await _db.StringSetAsync(Key(id), json);
    }
}