using DigitalDashboard.DAL.DTO;

namespace DigitalDashboard.JobManager.Repository
{
    public interface IDataImportRepository
    {
        Response ImportFromExcelAsync(IFormFile file);

        // TODO In Future
        Response ImportExcelDataIntoDB(IFormFile file);
    }
}
