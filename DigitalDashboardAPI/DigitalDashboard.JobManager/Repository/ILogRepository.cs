using DigitalDashboard.DAL.Models;

namespace DigitalDashboard.JobManager.Repository
{
    public interface ILogRepository
    {
        Task<List<Log>> GetAllLog();
        Task Logging(Log newLog);
    }
}
