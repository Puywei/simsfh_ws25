using System.Threading.Tasks;

namespace sims.Services
{
    public interface IEventLogger
    {
        Task LogEventAsync(string message, int severity = 1);
    }
}