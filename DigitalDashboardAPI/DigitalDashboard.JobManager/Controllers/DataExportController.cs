using DigitalDashboard.JobManager.Repository;
using DigitalDashboard.DAL.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System.ComponentModel;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace DigitalDashboard.JobManager.Controllers
{
    //[Route("api/[controller]")]
    [ApiController]
    public class DataExportController : ControllerBase
    {
        private readonly IDataExportRepository dataExportRepository;

        // Constructor:
        //    IDataExportRepository instance is retrieved from DI via constructor injection
        public DataExportController(IDataExportRepository dataExportRepository) =>
            this.dataExportRepository = dataExportRepository;

        // Summary:
        //    POST: This API is used to export regulatory SKU data to Excel using EPPlus_Version6.0.6
        [HttpPost, Route("digitaldashboard/DataExport/ExportRegulatorySKUToExcel")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BMSRegulatorySKUExport))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult<List<BMSRegulatorySKUExport>>> ExportRegulatorySKUToExcel([FromBody] BMSRegulatorySKUInput sKUInput)
        {
            string fileName = $"BMSRegulatorySKUData_{Guid.NewGuid():N}.xlsx";

            var list = await dataExportRepository.GetBMSReulatorySKUDataAsync(sKUInput);

            if (list.Count > 0)
            {
                var exportBytes = ExportToExcel<BMSRegulatorySKUExport>(list, fileName);
                return File(exportBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            else
            {
                return NoContent();
            }
        }

        private byte[] ExportToExcel<T>(List<T> data, string fileName)
        {
            // If you are a commercial business and have
            // purchased commercial licenses use the static property
            // LicenseContext of the ExcelPackage class:
            // ExcelPackage.LicenseContext = LicenseContext.Commercial;

            // If you use EPPlus in a noncommercial context
            // according to the Polyform Noncommercial license:
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using ExcelPackage excel = new ExcelPackage();

            ExcelWorksheet worksheet = excel.Workbook.Worksheets.Add(fileName);

            // POC API uses 'Microsoft.Office.Interop.Excel.TableStyles.Light1' for styling
            // but we are getting error in this. So that we are using 'OfficeOpenXml.Table.TableStyles.Light1'
            worksheet.Cells["A1"].LoadFromCollection(data, true, OfficeOpenXml.Table.TableStyles.Light1);

            return excel.GetAsByteArray();
        }

        #region Exporting data without using any third party libraries
        // Summary:
        //    POST: This API is used to export limited regulatory SKU data to Excel
        // Note:
        //    Here we are not using any third party library to export data to excel like EPPlus_Version6.0.6 
        //[HttpPost, Route("api/DataExport/ExportLimitedRegulatorySKUToExcel")]
        //[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BMSRegulatorySKUExport))]
        //[ProducesResponseType(StatusCodes.Status204NoContent)]
        //public async Task<ActionResult<List<BMSRegulatorySKUExport>>> ExportLimitedRegulatorySKUToExcel([FromBody] BMSRegulatorySKUInput sKUInput)
        //{
        //    var list = await dataExportRepository.GetBMSReulatorySKUDataAsync(sKUInput);

        //    if (list.Count > 0)
        //    {
        //        list = list.Take(10000).ToList();
        //        var exportBytes = ExportFile(list);

        //        return File(exportBytes, "application/vnd.ms-excel");
        //    }
        //    else
        //    {
        //        return NoContent();
        //    }
        //}

        //private byte[] ExportFile(List<BMSRegulatorySKUExport> list)
        //{
        //    StringBuilder str = new StringBuilder();
        //    str.Append("<table border=\"1px\">");
        //    str.Append("<tr style=\"font-weight: bold\">");
        //    str.Append("<td>SKU</td>");
        //    str.Append("<td>Product Description</td>");
        //    str.Append("<td>Offering Manager</td>");
        //    str.Append("<td>Region</td>");
        //    str.Append("<td>Sold To Country</td>");
        //    str.Append("<td>Fiscal Year</td>");
        //    str.Append("<td>Lead Supply Location</td>");
        //    str.Append("<td>Profit Ctr SBU Name</td>");
        //    str.Append("<td>SBU Name</td>");
        //    str.Append("<td>Origin Location</td>");
        //    str.Append("<td>Profit Ctr LOB Name Release Train</td>");
        //    str.Append("<td>LOB Name Line of Business</td>");
        //    str.Append("<td>Prod Family Brand</td>");
        //    str.Append("<td>Prod Line Sales Product Category</td>");
        //    str.Append("<td>PrdLn SubGrp Sales Product Type</td>");
        //    str.Append("<td>Quantity</td>");
        //    str.Append("<td>Revenue</td>");
        //    str.Append("</tr>");

        //    foreach (var item in list)
        //    {
        //        str.Append("<tr>");
        //        str.Append($"<td>{item.SKU.Trim().ToString()}</td>");
        //        str.Append($"<td>{item.ProductDescription}</td>");
        //        str.Append($"<td>{item.OfferingManager}</td>");
        //        str.Append($"<td>{item.Region}</td>");
        //        str.Append($"<td>{item.SoldToCountry}</td>");
        //        str.Append($"<td>{item.FiscalYear}</td>");
        //        str.Append($"<td>{item.LeadSupplyLocation}</td>");
        //        str.Append($"<td>{item.ProfitCtrSBUName}</td>");
        //        str.Append($"<td>{item.SBUName}</td>");
        //        str.Append($"<td>{item.OriginLocation}</td>");
        //        str.Append($"<td>{item.ProfitCtrLOBNameReleaseTrain}</td>");
        //        str.Append($"<td>{item.LOBNameLineofBusiness}</td>");
        //        str.Append($"<td>{item.ProdFamilyBrand}</td>");
        //        str.Append($"<td>{item.ProdLineSalesProductCategory}</td>");
        //        str.Append($"<td>{item.PrdLnSubGrpSalesProductType}</td>");
        //        str.Append($"<td>{item.Quantity}</td>");
        //        str.Append($"<td>{item.Revenue}</td>");
        //        str.Append("</tr>");
        //    }
        //    str.Append("</table>");
        //    Response.Clear();
        //    HttpContext.Response.Headers.Add("content-disposition", "attachment; filename=BMSRegulatorySKUData_" + DateTime.Now.ToString("ddMMyyyyhhmmss") + ".xls");
        //    this.Response.ContentType = "application/vnd.ms-excel";
        //    byte[] dataByte = System.Text.Encoding.UTF8.GetBytes(str.ToString());
        //    return dataByte;
        //}

        #endregion
    }
}
