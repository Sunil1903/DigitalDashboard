using DigitalDashboard.DAL.Models;

namespace DigitalDashboard.JobManager.Repository
{
    public interface IDataExportRepository
    {
        Task<List<BMSRegulatorySKUExport>> GetBMSReulatorySKUDataAsync(BMSRegulatorySKUInput sKUInput);
    }
}
