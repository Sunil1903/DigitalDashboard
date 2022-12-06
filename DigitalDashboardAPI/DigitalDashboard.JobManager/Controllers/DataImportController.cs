using DigitalDashboard.JobManager.Repository;
using DigitalDashboard.DAL.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DigitalDashboard.JobManager.Controllers
{
    //[Route("api/[controller]")]
    [ApiController]
    public class DataImportController : ControllerBase
    {
        private readonly IDataImportRepository dataImportRepository;

        // Constructor:
        //    IDataImportRepository instance is retrieved from DI via constructor injection
        public DataImportController(IDataImportRepository dataImportRepository) =>
            this.dataImportRepository = dataImportRepository;


        #region API Service: ExcelToDB
        // Summary:
        //    POST: This API used to import 'SKU' and 'regulation by country'
        //          data from Excel worksheets.
        [HttpPost, Route("digitaldashboard/DataImport/ExcelToDB")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<Response> ExcelToDB(IFormFile file)
        {
            var response = dataImportRepository.ImportFromExcelAsync(file);
            //return response.IsSuccess == false ? BadRequest(response) : Ok(response);
            return response == null ? BadRequest() : Ok(response);
        }
        #endregion

        #region New_Version
        //// Summary:
        ////    POST: This API used to import 'SKU' and 'regulation by country'
        ////          data from Excel worksheets.
        //[HttpPost, Route("digitaldashboard/DataImport/ExcelToDB/V2")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //public ActionResult<Response> ExcelToDBVersion2(IFormFile file)
        //{
        //    Response response = dataImportRepository.ImportExcelDataIntoDB(file);
        //    return response.IsSuccess == false ? BadRequest() : Ok(response);
        //}
        #endregion
    }
}
