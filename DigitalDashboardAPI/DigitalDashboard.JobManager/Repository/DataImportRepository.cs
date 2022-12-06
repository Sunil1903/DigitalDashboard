using DigitalDashboard.DAL.DTO;
using DigitalDashboard.DAL.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Data;
using System.Data.OleDb;
using DocumentFormat.OpenXml.Spreadsheet;
using ClosedXML.Excel;

namespace DigitalDashboard.JobManager.Repository
{
    public class DataImportRepository : IDataImportRepository
    {
        private readonly IMongoCollection<BMSRegulatorySKU> sKUCollection;
        private readonly IMongoCollection<BMSRegulatoryCountry> countryCollection;
        private readonly string connectionStrings = null!;
        private readonly string databaseName = null!;
        private readonly DataValidation dataValidation;

        // Constructor:
        //    IDigitalDashboardDatabaseSettings instance is retrieved from DI
        //    via constructor injection
        public DataImportRepository(IDigitalDashboardDatabaseSettings settings,
                                    IMongoClient mongoClient,
                                    DataValidation dataValidation)
        {
            var mongoDatabase = mongoClient.GetDatabase(settings.DatabaseName);
            sKUCollection = mongoDatabase.GetCollection<BMSRegulatorySKU>(settings.BMSRegulatorySKUCollectionName);
            countryCollection = mongoDatabase.GetCollection<BMSRegulatoryCountry>(settings.BMSRegulationsByCountryCollectionName);

            connectionStrings = settings.ConnectionString;
            databaseName = settings.DatabaseName;
            this.dataValidation = dataValidation;
        }

        #region Version #1

        // IMPORT DATA : Send a file with HttpRequest
        public Response ImportFromExcelAsync(IFormFile file)
        {
            Response response = new();
            try
            {
                string connectionStrings = string.Empty;
                string message = string.Empty;

                #region Data validation
                bool isDataValid = dataValidation.Validation(file,
                                                             ref message);
                if (!isDataValid)
                {
                    response.IsSuccess = false;
                    response.Message = message;
                    return response;
                }
                #endregion

                #region DropCollection_PostValidation
                // Drop all collection from MongoDB.
                // This code will need to be modified in the future
                bool isDropped = DropCollections(ref message);
                if (!isDropped)
                {
                    response.IsSuccess = false;
                    response.Message = message;
                    return response;
                }
                #endregion

                #region SetConnectionString_WithPath
                string fileExtension = Path.GetExtension(file.FileName);
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", file.FileName);
                switch (fileExtension)
                {
                    case ".xls":
                        connectionStrings = $"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={path};Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=1\"";
                        break;
                    case ".xlsx":
                        connectionStrings = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={path};Extended Properties=\"Excel 12.0 xml;HDR=Yes;IMEX=1\"";
                        break;
                }
                #endregion

                #region Create_OleDbConnectionAndImportData
                OleDbConnection objConn = new OleDbConnection(connectionStrings);

                objConn.Open();
                DataTable dtWorksheets = null;
                dtWorksheets = objConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                objConn.Close();
                if (dtWorksheets.Rows.Count > 0)
                {
                    foreach (DataRow row in dtWorksheets.Rows)
                    {
                        if (row["TABLE_NAME"].ToString().Replace("$", "").Replace("'", "") == "BMS Regulatory SKU Database" || row["TABLE_NAME"].ToString().Replace("$", "").Replace("'", "") == "Regulations by Country")
                        {
                            DataTable dt_sheet = null;
                            dt_sheet = GetSheetData(connectionStrings, row["TABLE_NAME"].ToString());
                            message = SaveDataTableToCollection(dt_sheet, row["TABLE_NAME"].ToString().Replace("$", "").Replace("'", ""), path);
                            response.Message = message;
                        }
                        else
                        {
                            response.Message = @"Invalid sheet name. The name of the sheet should be 'BMS Regulatory SKU Database' and 'Regulation by Country' only.";
                        }
                    }
                }
                #endregion
            }
            catch (Exception e)
            {
                response.IsSuccess = false;
                response.Message = e.Message;
                return response;
            }
            return response;
        }

        #region Implementation_DropCollections
        private bool DropCollections(ref string message)
        {
            bool isDropped = true;
            try
            {
                string sKUCollection = "BMS Regulatory SKU Database";
                string countryCollection = "Regulations by Country";
                var client = new MongoClient(connectionStrings);
                var db = client.GetDatabase(databaseName);

                if (db != null)
                {
                    db.DropCollection(sKUCollection);
                    db.DropCollection(countryCollection);
                }
                message = "The collection has been dropped successfully!";
            }
            catch (Exception e)
            {
                message = e.Message;
                return false;
                //throw;
            }
            return isDropped;
        }
        #endregion

        #region Implementation_SaveDataTableToMongoDbCollection
        public string SaveDataTableToCollection(DataTable dt,
                                                string sheetName,
                                                string filepath)
        {
            string msg = "";
            string JSONString = string.Empty;
            List<BsonDocument> CheckCount;
            List<BMSRegulatorySKU> BMSSKUData = new List<BMSRegulatorySKU>();
            List<BMSRegulatoryCountry> BMSCountryData = new List<BMSRegulatoryCountry>();
            var client = new MongoClient(connectionStrings);
            var database = client.GetDatabase(databaseName);
            var collection = database.GetCollection<BsonDocument>(sheetName);
            try
            {
                if (dt.Rows.Count > 0)
                {
                    DataColumn DcActive = new DataColumn("Active", typeof(string));
                    DcActive.DefaultValue = "true";

                    DataColumn DcUpdatedDateTime = new DataColumn("UpdateDatetime");
                    DcUpdatedDateTime.DefaultValue = DateTime.Now;

                    DataColumn DcUserID = new DataColumn("UserID", typeof(string));
                    DcUserID.DefaultValue = 0;

                    DataColumn DcStatus = new DataColumn("Status", typeof(string));
                    DcStatus.DefaultValue = "I";

                    dt.Columns.Add(DcActive);
                    dt.Columns.Add(DcUpdatedDateTime);
                    dt.Columns.Add(DcStatus);
                    dt.Columns.Add(DcUserID);
                    dt.AcceptChanges();

                    CheckCount = collection.Find(a => true).ToList();
                    int count = CheckCount.Count();
                    JSONString = Newtonsoft.Json.JsonConvert.SerializeObject(dt);

                    if (sheetName == "BMS Regulatory SKU Database")
                    {
                        try
                        {
                            BMSSKUData = BsonSerializer.Deserialize<List<BMSRegulatorySKU>>(JSONString);
                        }
                        catch (Exception e)
                        {
                            //logger.LogError(e.Message);
                        }
                    }

                    if (sheetName == "Regulations by Country")
                    {
                        try
                        {
                            BMSCountryData = GetExcelDataWithHyperlinks(filepath, sheetName);
                            //BMSCountryData = BsonSerializer.Deserialize<List<BMSRegulatoryCountry>>(JSONString);
                        }
                        catch (Exception ex)
                        {
                            //logger.LogError(ex.Message);
                        }
                    }
                    if (count == 0)
                    {
                        if (sheetName == "BMS Regulatory SKU Database")
                        {
                            msg = Create(BMSSKUData);
                        }
                        if (sheetName == "Regulations by Country")
                        {
                            msg = Create(BMSCountryData);
                        }
                    }
                    else
                    {
                        if (sheetName == "BMS Regulatory SKU Database")
                        {
                            List<BMSRegulatorySKU> databaselistdata = new List<BMSRegulatorySKU>();
                            List<BMSRegulatorySKU> datatablelistdata = new List<BMSRegulatorySKU>();

                            var collection11 = database.GetCollection<BMSRegulatorySKU>(sheetName);

                            databaselistdata = collection11.Find(emp1 => true).ToList();

                            datatablelistdata = (from DataRow row in dt.Rows
                                                 select new BMSRegulatorySKU()
                                                 {
                                                     SKU = row["SKU"].ToString(),
                                                     ProductDescription = row["Product Description"].ToString(),
                                                     OfferingManager = row["Offering Manager"].ToString(),
                                                     Region = row["Region"].ToString(),
                                                     SoldToCountry = row["Sold-To Country (TRX) Name"].ToString(),
                                                     FiscalYear = row["Fiscal Year"].ToString(),
                                                     Quantity = Convert.ToDouble(row["Quantity"]),
                                                     Revenue = Convert.ToDouble(row["Revenue"]),
                                                     OriginLocation = row["Origin Location"].ToString(),
                                                     LeadSupplyLocation = row["Lead Supply Location"].ToString(),
                                                     ProfitCtrSBUName = row["Profit Ctr SBU Name"].ToString(),
                                                     SBUName = row["SBU Name"].ToString(),
                                                     ProfitCtrLOBNameReleaseTrain = row["Profit Ctr LOB Name"].ToString(),
                                                     LOBNameLineofBusiness = row["LOB Name"].ToString(),
                                                     ProdFamilyBrand = row["Prod Family (Sales) Name"].ToString(),
                                                     ProdLineSalesProductCategory = row["Prod Line (Sales) Name"].ToString(),
                                                     PrdLnSubGrpSalesProductType = row["PrdLn SubGrp (Sales) Name"].ToString(),
                                                     Active = row["Active"].ToString(),
                                                     UpdateDatetime = row["UpdateDatetime"].ToString(),
                                                     UserID = row["UserID"].ToString(),
                                                     Status = row["Status"].ToString()
                                                 }).ToList();

                            List<BMSRegulatorySKU> querycheckcommon1 = (from pr in datatablelistdata
                                                                        join p in databaselistdata on pr.SKU equals p.SKU
                                                                        select pr).ToList();

                            List<BMSRegulatorySKU> querycheckdatabasetolist = databaselistdata.Except(from pr in databaselistdata
                                                                                                      join p in datatablelistdata on pr.SKU equals p.SKU
                                                                                                      select pr).ToList();

                            List<BMSRegulatorySKU> querychecklisttodatabase = datatablelistdata.Except(from pr in datatablelistdata
                                                                                                       join p in databaselistdata on pr.SKU equals p.SKU
                                                                                                       select pr).ToList();
                            if (querycheckcommon1.Count > 0)
                            {
                                List<BMSRegulatorySKU> getcommonrecords = (from pr in databaselistdata
                                                                           join p in datatablelistdata on pr.SKU equals p.SKU
                                                                           select pr).ToList();
                                foreach (var x in getcommonrecords)
                                {
                                    querycheckcommon1.First(d => d.SKU == x.SKU).id = x.id;
                                    querycheckcommon1.First(d => d.SKU == x.SKU).Status = "U";
                                    querycheckcommon1.First(d => d.SKU == x.SKU).UpdateDatetime = DateTime.Now.ToString();

                                }
                                msg = UpdateAll(querycheckcommon1);
                            }
                            if (querycheckdatabasetolist.Count > 0)
                            {
                                foreach (var x in querycheckdatabasetolist)
                                {
                                    querycheckdatabasetolist.First(d => d.SKU == x.SKU).id = x.id;
                                    querycheckdatabasetolist.First(d => d.SKU == x.SKU).Active = "false";
                                    querycheckdatabasetolist.First(d => d.SKU == x.SKU).Status = "D";
                                    querycheckdatabasetolist.First(d => d.SKU == x.SKU).UpdateDatetime = DateTime.Now.ToString();
                                }
                                msg = UpdateAll(querycheckdatabasetolist);
                            }
                            if (querychecklisttodatabase.Count > 0)
                            {
                                foreach (var x in querychecklisttodatabase)
                                {
                                    querychecklisttodatabase.First(d => d.SKU == x.SKU).Active = "true";
                                    querychecklisttodatabase.First(d => d.SKU == x.SKU).Status = "I";
                                    querychecklisttodatabase.First(d => d.SKU == x.SKU).UpdateDatetime = DateTime.Now.ToString();
                                }
                                msg = Create(querychecklisttodatabase);
                            }
                        }
                        if (sheetName == "Regulations by Country")
                        {
                            List<BMSRegulatoryCountry> databaselistdata = new List<BMSRegulatoryCountry>();
                            List<BMSRegulatoryCountry> datatablelistdata = new List<BMSRegulatoryCountry>();

                            var collection11 = database.GetCollection<BMSRegulatoryCountry>(sheetName);

                            databaselistdata = collection11.Find(emp1 => true).ToList();

                            datatablelistdata = (from DataRow row in dt.Rows
                                                 select new BMSRegulatoryCountry()
                                                 {
                                                     Region = row["Region"].ToString(),
                                                     Country = row["Country"].ToString(),
                                                     RegulatoryCategory = row["Regulatory Category"].ToString(),
                                                     MandatoryCertificationScheme = row["Mandatory Certification Scheme"].ToString(),
                                                     OptionalCertificationScheme = row["Optional Certification Scheme"].ToString(),
                                                     TestingRequired = row["Testing Required?"].ToString(),
                                                     CustomsConsiderations = row["Customs Considerations"].ToString(),
                                                     MarkingRequirements = row["Marking Requirements"].ToString(),
                                                     Contact = row["Contact"].ToString(),
                                                     AgencyReferences = row["Agency References"].ToString(),
                                                     Notes = row["Notes"].ToString(),
                                                     Details = row["Details"].ToString(),
                                                     Active = row["Active"].ToString(),
                                                     UpdateDatetime = row["UpdateDatetime"].ToString(),
                                                     UserID = row["UserID"].ToString(),
                                                     Status = row["Status"].ToString()
                                                 }).ToList();

                            List<BMSRegulatoryCountry> querycheckcommon = (from pr in datatablelistdata
                                                                           join p in databaselistdata on
                                                                           new { Organization_Type = pr.Region, Cost_Type = pr.Country, x = pr.RegulatoryCategory }
                                                                           equals
                                                                           new { Organization_Type = p.Region, Cost_Type = p.Country, x = p.RegulatoryCategory }
                                                                           select pr).ToList();

                            List<BMSRegulatoryCountry> querycheckdatabasetolist = databaselistdata.Except(from p in databaselistdata
                                                                                                          join pr in datatablelistdata on
                                                                                                          new { Organization_Type = p.Region, Cost_Type = p.Country, x = p.RegulatoryCategory }
                                                                                                          equals
                                                                                                          new { Organization_Type = pr.Region, Cost_Type = pr.Country, x = pr.RegulatoryCategory }
                                                                                                          select p).ToList();

                            List<BMSRegulatoryCountry> querychecklisttodatabase = databaselistdata.Except(from p in datatablelistdata
                                                                                                          join pr in databaselistdata on
                                                                                                          new { Organization_Type = p.Region, Cost_Type = p.Country, x = p.RegulatoryCategory }
                                                                                                          equals
                                                                                                          new { Organization_Type = pr.Region, Cost_Type = pr.Country, x = pr.RegulatoryCategory }
                                                                                                          select pr).ToList();
                            if (querycheckcommon.Count > 0)
                            {
                                List<BMSRegulatoryCountry> getcommonrecords = (from pr in databaselistdata
                                                                               join p in datatablelistdata on
                                                                               new { Organization_Type = pr.Region, Cost_Type = pr.Country, x = pr.RegulatoryCategory }
                                                                               equals
                                                                               new { Organization_Type = p.Region, Cost_Type = p.Country, x = p.RegulatoryCategory }
                                                                               select pr).ToList();
                                foreach (var x in getcommonrecords)
                                {
                                    querycheckcommon.First(d => d.Country == x.Country && d.Region == x.Region && d.RegulatoryCategory == x.RegulatoryCategory).id = x.id;
                                    querycheckcommon.First(d => d.Country == x.Country && d.Region == x.Region && d.RegulatoryCategory == x.RegulatoryCategory).Status = "U";
                                    querycheckcommon.First(d => d.Country == x.Country && d.Region == x.Region && d.RegulatoryCategory == x.RegulatoryCategory).UpdateDatetime = DateTime.Now.ToString();
                                }
                                msg = UpdateAll(querycheckcommon);
                            }
                            if (querycheckdatabasetolist.Count > 0)
                            {
                                foreach (var x in querycheckdatabasetolist)
                                {
                                    querycheckdatabasetolist.First(d => d.Country == x.Country && d.Region == x.Region && d.RegulatoryCategory == x.RegulatoryCategory).id = x.id;
                                    querycheckdatabasetolist.First(d => d.Country == x.Country && d.Region == x.Region && d.RegulatoryCategory == x.RegulatoryCategory).Active = "false";
                                    querycheckdatabasetolist.First(d => d.Country == x.Country && d.Region == x.Region && d.RegulatoryCategory == x.RegulatoryCategory).Status = "D";
                                    querycheckdatabasetolist.First(d => d.Country == x.Country && d.Region == x.Region && d.RegulatoryCategory == x.RegulatoryCategory).UpdateDatetime = DateTime.Now.ToString();
                                }
                                msg = UpdateAll(querycheckdatabasetolist);
                            }
                            if (querychecklisttodatabase.Count > 0)
                            {
                                foreach (var x in querychecklisttodatabase)
                                {
                                    querychecklisttodatabase.First(d => d.Country == x.Country && d.Region == x.Region && d.RegulatoryCategory == x.RegulatoryCategory).Active = "true";
                                    querychecklisttodatabase.First(d => d.Country == x.Country && d.Region == x.Region && d.RegulatoryCategory == x.RegulatoryCategory).Status = "I";
                                    querychecklisttodatabase.First(d => d.Country == x.Country && d.Region == x.Region && d.RegulatoryCategory == x.RegulatoryCategory).UpdateDatetime = DateTime.Now.ToString();
                                }
                                msg = Create(querychecklisttodatabase);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return msg = "Duplicate Record Found Please Upload Valid file";
            }
            return msg;
        }
        #endregion

        #region Implementation_InsertAndUpdateToMongoDb
        public string UpdateAll<T>(List<T> newData)
        {
            if (newData is List<BMSRegulatorySKU>)
            {
                List<BMSRegulatorySKU> newData1 = new List<BMSRegulatorySKU>();
                newData1 = (List<BMSRegulatorySKU>)(IEnumerable<BMSRegulatorySKU>)newData;
                foreach (BMSRegulatorySKU data in newData1)
                {
                    sKUCollection.ReplaceOne(student => student.SKU == data.SKU, data);
                }
            }
            if (newData is List<BMSRegulatoryCountry>)
            {
                List<BMSRegulatoryCountry> newData1 = new List<BMSRegulatoryCountry>();
                newData1 = (List<BMSRegulatoryCountry>)(IEnumerable<BMSRegulatoryCountry>)newData;
                foreach (var data in newData1)
                {
                    countryCollection.ReplaceOne(student => student.Region == data.Region && student.Country == data.Country && student.RegulatoryCategory == data.RegulatoryCategory, data);
                }

            }
            return "File Uploaded Successfully!";
        }

        public string UpdateMany(string sku, List<BMSRegulatorySKU> student)
        {
            sKUCollection.BulkWrite((IEnumerable<WriteModel<BMSRegulatorySKU>>)student);
            return "Data Updated";
        }

        public string Create<T>(List<T> newData)
        {
            if (newData is List<BMSRegulatorySKU>)
            {
                sKUCollection.InsertMany((IEnumerable<BMSRegulatorySKU>)newData);
            }
            if (newData is List<BMSRegulatoryCountry>)
            {
                countryCollection.InsertMany((IEnumerable<BMSRegulatoryCountry>)newData);
            }
            return "File Uploaded Successfully!";
        }
        #endregion

        #region Implementation_GetSheetData
        private DataTable GetSheetData(string strConn, string sheet)
        {
            string query = "select * from [" + sheet + "]";

            OleDbConnection objConn;
            OleDbDataAdapter oleDA;
            DataTable dt = new DataTable();
            objConn = new OleDbConnection(strConn);
            objConn.Open();
            oleDA = new OleDbDataAdapter(query, objConn);
            oleDA.Fill(dt);
            objConn.Close();
            oleDA.Dispose();
            objConn.Dispose();
            return dt;
        }
        #endregion

        #region Implementation_GetExcelDataWithHyperlinks
        public List<BMSRegulatoryCountry> GetExcelDataWithHyperlinks(string fileppathath,
                                                                     string sheetname)
        {
            List<BMSRegulatoryCountry> bmsregulatorycountry = new List<BMSRegulatoryCountry>();
            using (XLWorkbook workBook = new XLWorkbook(fileppathath))
            {
                //Read the first Sheet from Excel file.
                IXLWorksheet workSheet = workBook.Worksheet(2);
                bool firstRow = true;
                foreach (IXLRow row in workSheet.Rows())
                {
                    //Use the first row to add columns to DataTable.
                    if (firstRow)
                    {
                        foreach (IXLCell cell in row.Cells())
                        {
                            string value = cell.Value.ToString();
                        }
                        firstRow = false;
                    }
                    else
                    {
                        BMSRegulatoryCountry bmscountrydata = new BMSRegulatoryCountry();
                        int i = 0;
                        foreach (IXLCell cell in row.Cells())
                        {

                            switch (cell.Address.ColumnLetter)
                            {
                                case "A":
                                    if (cell.HasHyperlink)
                                    {
                                        string link = cell.Value.ToString() + ";" + cell.GetHyperlink().ExternalAddress.ToString();
                                        bmscountrydata.Region = link;
                                    }
                                    else
                                    {
                                        bmscountrydata.Region = cell.Value.ToString();
                                    }
                                    break;
                                case "B":
                                    if (cell.HasHyperlink)
                                    {
                                        string link = cell.Value.ToString() + ";" + cell.GetHyperlink().ExternalAddress.ToString();
                                        bmscountrydata.Country = link;
                                    }
                                    else
                                    {
                                        bmscountrydata.Country = cell.Value.ToString();
                                    }
                                    break;
                                case "C":
                                    if (cell.HasHyperlink)
                                    {
                                        string link = cell.Value.ToString() + ";" + cell.GetHyperlink().ExternalAddress.ToString();
                                        bmscountrydata.RegulatoryCategory = link;
                                    }
                                    else
                                    {
                                        bmscountrydata.RegulatoryCategory = cell.Value.ToString();
                                    }
                                    break;
                                case "D":
                                    if (cell.HasHyperlink)
                                    {
                                        string link = cell.Value.ToString() + ";" + cell.GetHyperlink().ExternalAddress.ToString();
                                        bmscountrydata.MandatoryCertificationScheme = link;
                                    }
                                    else
                                    {
                                        bmscountrydata.MandatoryCertificationScheme = cell.Value.ToString();
                                    }
                                    break;
                                case "E":
                                    if (cell.HasHyperlink)
                                    {
                                        string link = cell.Value.ToString() + ";" + cell.GetHyperlink().ExternalAddress.ToString();
                                        bmscountrydata.OptionalCertificationScheme = link;
                                    }
                                    else
                                    {
                                        bmscountrydata.OptionalCertificationScheme = cell.Value.ToString();
                                    }
                                    break;
                                case "F":
                                    if (cell.HasHyperlink)
                                    {
                                        string link = cell.Value.ToString() + ";" + cell.GetHyperlink().ExternalAddress.ToString();
                                        bmscountrydata.TestingRequired = link;
                                    }
                                    else
                                    {
                                        bmscountrydata.TestingRequired = cell.Value.ToString();
                                    }
                                    break;
                                case "G":
                                    if (cell.HasHyperlink)
                                    {
                                        string link = cell.Value.ToString() + ";" + cell.GetHyperlink().ExternalAddress.ToString();
                                        bmscountrydata.CustomsConsiderations = link;
                                    }
                                    else
                                    {
                                        bmscountrydata.CustomsConsiderations = cell.Value.ToString();
                                    }
                                    break;
                                case "H":
                                    if (cell.HasHyperlink)
                                    {
                                        string link = cell.Value.ToString() + ";" + cell.GetHyperlink().ExternalAddress.ToString();
                                        bmscountrydata.MarkingRequirements = link;
                                    }
                                    else
                                    {
                                        bmscountrydata.MarkingRequirements = cell.Value.ToString();
                                    }
                                    break;
                                case "I":
                                    if (cell.HasHyperlink)
                                    {
                                        string link = cell.Value.ToString() + ";" + cell.GetHyperlink().ExternalAddress.ToString();
                                        bmscountrydata.Contact = link;
                                    }
                                    else
                                    {
                                        bmscountrydata.Contact = cell.Value.ToString();
                                    }
                                    break;
                                case "J":
                                    if (cell.HasHyperlink)
                                    {
                                        string link = cell.Value.ToString() + ";" + cell.GetHyperlink().ExternalAddress.ToString();
                                        bmscountrydata.AgencyReferences = link;
                                    }
                                    else
                                    {
                                        bmscountrydata.AgencyReferences = cell.Value.ToString();
                                    }
                                    break;
                                case "K":
                                    if (cell.HasHyperlink)
                                    {
                                        string link = cell.Value.ToString() + ";" + cell.GetHyperlink().ExternalAddress.ToString();
                                        bmscountrydata.Notes = link;
                                    }
                                    else
                                    {
                                        bmscountrydata.Notes = cell.Value.ToString();
                                    }
                                    break;
                                case "L":
                                    if (cell.HasHyperlink)
                                    {
                                        string link = cell.Value.ToString() + ";" + cell.GetHyperlink().ExternalAddress.ToString();
                                        bmscountrydata.Details = link;
                                    }
                                    else
                                    {
                                        bmscountrydata.Details = cell.Value.ToString();
                                    }
                                    break;
                                default:
                                    Console.WriteLine("Not Known");
                                    break;
                            }


                            //   string value = cell.Value.ToString();
                            i++;
                        }
                        bmscountrydata.Status = "I";
                        bmscountrydata.Active = "true";
                        bmscountrydata.UpdateDatetime = DateTime.Now.ToString();
                        bmscountrydata.UserID = "0";
                        bmsregulatorycountry.Add(bmscountrydata);
                    }
                }

                return bmsregulatorycountry;
            }
        }
        #endregion

        #endregion

        #region Version #2 - TODO In Future
        // Under development
        public Response ImportExcelDataIntoDB(IFormFile file)
        {
            Response response = new();
            try
            {
                string message = string.Empty;
                bool isDataValid = dataValidation.Validation(file,
                                                             ref message);
                if (!isDataValid)
                {
                    response.IsSuccess = false;
                    response.Message = message;
                    return response;
                }

                // Coding
                // ...
                // ...
                //  Coding

                //response.Message = "File Uploaded Successfully!";
            }
            catch (Exception e)
            {
                response.IsSuccess = false;
                response.Message = e.Message;
                return response;
            }
            return response;
        }
        #endregion
    }
}
