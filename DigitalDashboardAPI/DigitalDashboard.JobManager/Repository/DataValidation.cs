using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DigitalDashboard.JobManager.Entities;
using SharpCompress.Common;

namespace DigitalDashboard.JobManager.Repository
{
    public class DataValidation
    {
        public bool Validation(IFormFile file, ref string message)
        {
            bool isValid = true;

            string exceptionMessage = string.Empty;
            string filePath = string.Empty;

            #region FileTypeAndSizeValidation
            // Verify the file type of the uploaded file.
            bool isFileVerified = FileTypeAndSizeValidation(file, ref exceptionMessage);
            if (!isFileVerified)
            {
                message = exceptionMessage;
                return false;
            }
            #endregion

            #region SaveFileToDirectory
            // Save uploaded file to directory
            bool isFileSaved = SaveFileToDirectory(file,
                                                   ref filePath,
                                                   ref exceptionMessage);
            if (!isFileSaved)
            {
                message = exceptionMessage;
                return false;
            }
            #endregion

            #region SheetAndHeaderValidation
            // Verify the number of sheets and the name of each sheet.
            bool isSheetVerified = SheetAndHeaderValidation(filePath, ref exceptionMessage);
            if (!isSheetVerified)
            {
                message = exceptionMessage;
                return false;
            }
            #endregion

            message = "The Excel file has been verified successfully.";
            return isValid;
        }

        #region Implementation_FileTypeAndSizeValidation
        // Verifying that the file type must be .xls and .xlsx
        private static bool FileTypeAndSizeValidation(IFormFile file, ref string message)
        {
            bool isFileVerified = true;

            string fileName = file.FileName;
            FileInfo info = new(fileName);

            string fileExtension = Path.GetExtension(fileName);

            // Validating excel file format
            if (fileExtension.ToLower() != ".xls" && fileExtension.ToLower() != ".xlsx")
            {
                message = @"Invalid file format. Only Excel (.xlsx or .xls) file is allowed to be uploaded!";
                isFileVerified = false;
            }

            // Validation of file size while uploading
            long length = info.FullName.Length;
            if (length == 0)
            {
                message = @"File size should not be empty!";
                isFileVerified = false;
            }

            return isFileVerified;
        }
        #endregion

        #region Implementation_SaveFileToDirectory
        // Saving uploaded file to directory
        private static bool SaveFileToDirectory(IFormFile file,
                                                ref string filePath,
                                                ref string exceptionMessage)
        {
            bool isSaved = true;

            string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads");

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            filePath = Path.Combine(directoryPath, file.FileName);

            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception)
                {
                    exceptionMessage = $"Uploading Error - invalid file path: {filePath}";
                    return false;
                }
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            return isSaved;
        }
        #endregion

        #region Implementation_SheetAndHeaderValidation
        // Verifying the number of sheets and the name of each sheet.
        private static bool SheetAndHeaderValidation(string filePath, ref string exceptionMessage)
        {
            bool isSheetVerified = true;
            try
            {
                var results = GetAllWorksheets(filePath);
                int noOfSheets = results.Count();
                if (noOfSheets != 2)
                {
                    exceptionMessage = @"Invalid number of SHEETS in uploaded excel. The number of sheets should be 2 only.";
                    return false;
                }

                foreach (Sheet item in results.Cast<Sheet>())
                {
                    string sheetName = item.Name;

                    // Case insensitive will be implemented in future
                    // string customSheetName = sheetName.RemoveSpecialCharacters().RemoveSpaces().ToLower();
                    // string sKUSheet = SheetNameEnum.BMSRegulatorySKUDatabase.ToString().ToLower();
                    // string countrySheet = SheetNameEnum.RegulationsByCountry.ToString().ToLower();

                    string customSheetName = sheetName.RemoveSpecialCharacters().RemoveSpaces();
                    string sKUSheet = SheetNameEnum.BMSRegulatorySKUDatabase.ToString();
                    string countrySheet = SheetNameEnum.RegulationsbyCountry.ToString();

                    if (customSheetName == sKUSheet || customSheetName == countrySheet)
                    {
                        var headers = GetSheetHeaders(filePath, sheetName);

                        if (headers.Count > 0)
                        {
                            // The number of columns should be 16 for SKU regulatory.
                            if (customSheetName == sKUSheet)
                            {
                                if (headers.Count != 16)
                                {
                                    exceptionMessage = $"Invalid number of columns in SHEET: '{sheetName}'. The number of columns should be 16 only.";
                                    return false;
                                }

                                // Verifying the header name of
                                // the SKU sheet
                                foreach (var header in headers)
                                {
                                    string orignalHeader = header.Trim();
                                    string customHeader = header.RemoveSpecialCharacters().ToLower();

                                    if (customHeader == SKUHeadersEnum.SKU.ToString().ToLower() ||
                                        customHeader == SKUHeadersEnum.ProductDescription.ToString().ToLower() ||
                                        customHeader == SKUHeadersEnum.OfferingManager.ToString().ToLower() ||
                                        customHeader == SKUHeadersEnum.Region.ToString().ToLower() ||
                                        customHeader == SKUHeadersEnum.SoldToCountryTRXName.ToString().ToLower() ||
                                        customHeader == SKUHeadersEnum.FiscalYear.ToString().ToLower() ||
                                        customHeader == SKUHeadersEnum.LeadSupplyLocation.ToString().ToLower() ||
                                        customHeader == SKUHeadersEnum.ProfitCtrSBUName.ToString().ToLower() ||
                                        customHeader == SKUHeadersEnum.SBUName.ToString().ToLower() ||
                                        customHeader == SKUHeadersEnum.ProfitCtrLOBName.ToString().ToLower() ||
                                        customHeader == SKUHeadersEnum.LOBName.ToString().ToLower() ||
                                        customHeader == SKUHeadersEnum.ProdFamilySalesName.ToString().ToLower() ||
                                        customHeader == SKUHeadersEnum.ProdLineSalesName.ToString().ToLower() ||
                                        customHeader == SKUHeadersEnum.PrdLnSubGrpSalesName.ToString().ToLower() ||
                                        customHeader == SKUHeadersEnum.Quantity.ToString().ToLower() ||
                                        customHeader == SKUHeadersEnum.Revenue.ToString().ToLower())
                                    {
                                        exceptionMessage = $"HEADERS are valid.";
                                    }
                                    else
                                    {
                                        exceptionMessage = $"HEADER: '{orignalHeader}' is invalid in SHEET: {sheetName}.";
                                        return false;
                                    }
                                }
                            }

                            //The number of columns should be 12
                            //for Country regulations.
                            if (customSheetName == countrySheet)
                            {
                                if (headers.Count != 12)
                                {
                                    exceptionMessage = $"Invalid number of columns in SHEET: '{sheetName}'. The number of columns should be 12 only.";
                                    return false;
                                }

                                // Verifying the header name of
                                // the country sheet
                                foreach (var header in headers)
                                {
                                    string orignalHeader = header.Trim();
                                    string customHeader = header.RemoveSpecialCharacters().ToLower();
                                    
                                    if (customHeader == CountryHeadersEnum.Region.ToString().ToLower() ||
                                        customHeader == CountryHeadersEnum.Country.ToString().ToLower() ||
                                        customHeader == CountryHeadersEnum.RegulatoryCategory.ToString().ToLower() ||
                                        customHeader == CountryHeadersEnum.MandatoryCertificationScheme.ToString().ToLower() ||
                                        customHeader == CountryHeadersEnum.OptionalCertificationScheme.ToString().ToLower() ||
                                        customHeader == CountryHeadersEnum.TestingRequired.ToString().ToLower() ||
                                        customHeader == CountryHeadersEnum.CustomsConsiderations.ToString().ToLower() ||
                                        customHeader == CountryHeadersEnum.MarkingRequirements.ToString().ToLower() ||
                                        customHeader == CountryHeadersEnum.Contact.ToString().ToLower() ||
                                        customHeader == CountryHeadersEnum.AgencyReferences.ToString().ToLower() ||
                                        customHeader == CountryHeadersEnum.Notes.ToString().ToLower() ||
                                        customHeader == CountryHeadersEnum.Details.ToString().ToLower())
                                    {
                                        exceptionMessage = $"HEADERS are valid.";
                                    }
                                    else
                                    {
                                        exceptionMessage = $"HEADER: '{orignalHeader}' is invalid in SHEET: {sheetName}.";
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        exceptionMessage = $"Invalid SHEET NAME: '{sheetName}' in the uploaded excel. The name of the sheet should be 'BMS Regulatory SKU Database' and 'Regulation by Country' only.";
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                exceptionMessage = @"Something went wrong please try again!";
                throw;
            }
            return isSheetVerified;
        }

        private static List<string> GetSheetHeaders(string filePath, string sheetName)
        {
            List<string> Headers = new();

            // Open the spreadsheet document for read-only access.
            using SpreadsheetDocument document = SpreadsheetDocument.Open(filePath, false);

            // Retrieve a reference to the workbook part.
            WorkbookPart wbPart = document.WorkbookPart;

            // Find the sheet with the supplied name, and then use that 
            // Sheet object to retrieve a reference to the first worksheet.
            Sheet theSheet = wbPart.Workbook.Descendants<Sheet>().Where(s => s.Name == sheetName).FirstOrDefault();

            // Retrieve a reference to the worksheet part.
            WorksheetPart wsPart = (WorksheetPart)(wbPart.GetPartById(theSheet.Id));

            Worksheet worksheet = (wsPart as WorksheetPart).Worksheet;

            IEnumerable<Row> rows = worksheet.GetFirstChild<SheetData>().Descendants<Row>();

            var topRow = rows.FirstOrDefault();
            if (topRow != null)
            {
                foreach (Cell cell in topRow.Descendants<Cell>())
                {
                    var colunmName = GetCellValue(document, cell);
                    Headers.Add(colunmName);
                }
            }

            return Headers;
        }

        public static Sheets? GetAllWorksheets(string filePath)
        {
            Sheets? theSheets = null;

            using (SpreadsheetDocument document =
                SpreadsheetDocument.Open(filePath, false))
            {
                WorkbookPart? wbPart = document.WorkbookPart;
                if (wbPart != null)
                {
                    theSheets = wbPart.Workbook.Sheets;
                }
            }
            return theSheets;
        }

        private static string GetCellValue(SpreadsheetDocument document, Cell cell)
        {
            string value = cell.CellValue.InnerText;
            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                return document.WorkbookPart.SharedStringTablePart.SharedStringTable.ChildElements.GetItem(int.Parse(value)).InnerText;
            }
            return value;
        }
        #endregion
    }
}
