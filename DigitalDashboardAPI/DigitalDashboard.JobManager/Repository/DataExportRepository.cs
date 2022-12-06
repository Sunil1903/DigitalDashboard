using DigitalDashboard.DAL.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace DigitalDashboard.JobManager.Repository
{
    public class DataExportRepository : IDataExportRepository
    {
        private readonly IMongoCollection<BMSRegulatorySKU> sKUCollection;

        // Constructor:
        //    IDigitalDashboardDatabaseSettings instance is retrieved from DI
        //    via constructor injection    
        public DataExportRepository(IDigitalDashboardDatabaseSettings settings,
                                    IMongoClient mongoClient,
                                    DataValidation dataValidation)
        {
            var mongoDatabase = mongoClient.GetDatabase(settings.DatabaseName);
            sKUCollection = mongoDatabase.GetCollection<BMSRegulatorySKU>(settings.BMSRegulatorySKUCollectionName);
        }

        public async Task<List<BMSRegulatorySKUExport>> GetBMSReulatorySKUDataAsync(BMSRegulatorySKUInput sKUInput)
        {
            List<BMSRegulatorySKUExport> bMSRegulatorySKUExports = new List<BMSRegulatorySKUExport>();
            try
            {
                #region Input_Parameters

                string?[] regionList = { null };
                string?[] SKUList = { null };
                string?[] ProductDescriptionList = { null };
                string?[] OfferingManagerList = { null };
                string?[] SoldToCountryList = { null };
                string?[] FiscalYearList = { null };

                if (!string.IsNullOrEmpty(sKUInput.SKU))
                {
                    var masterSet = new HashSet<string>(sKUInput.SKU.Split(BaseClass.CharacterDelimiter));
                }
                if (!string.IsNullOrEmpty(sKUInput.Region))
                {
                    regionList = sKUInput.Region.Split(BaseClass.CharacterDelimiter);
                }
                if (!string.IsNullOrEmpty(sKUInput.SKU))
                {
                    SKUList = sKUInput.SKU.Split(BaseClass.CharacterDelimiter);
                }
                if (!string.IsNullOrEmpty(sKUInput.ProductDescription))
                {
                    ProductDescriptionList = sKUInput.ProductDescription.Split(BaseClass.CharacterDelimiter);
                }
                if (!string.IsNullOrEmpty(sKUInput.OfferingManager))
                {
                    OfferingManagerList = sKUInput.OfferingManager.Split(BaseClass.CharacterDelimiter);
                }
                if (!string.IsNullOrEmpty(sKUInput.SoldToCountry))
                {
                    SoldToCountryList = sKUInput.SoldToCountry.Split(BaseClass.CharacterDelimiter);
                }
                if (!string.IsNullOrEmpty(sKUInput.FiscalYear))
                {
                    FiscalYearList = sKUInput.FiscalYear.Split(BaseClass.CharacterDelimiter);
                }
                #endregion

                #region Fetching_AllRecords
                var list = await sKUCollection
                                 .Find(x => x.Active.ToLower() == "true"
                                 && (string.IsNullOrEmpty(sKUInput.SKU) || SKUList.Contains(x.SKU))
                                 && (string.IsNullOrEmpty(sKUInput.ProductDescription.Trim()) || ProductDescriptionList.Contains(x.ProductDescription))
                                 && (string.IsNullOrEmpty(sKUInput.OfferingManager.Trim()) || OfferingManagerList.Contains(x.OfferingManager))
                                 && (string.IsNullOrEmpty(sKUInput.SoldToCountry.Trim()) || SoldToCountryList.Contains(x.SoldToCountry))
                                 && (string.IsNullOrEmpty(sKUInput.FiscalYear.Trim()) || FiscalYearList.Contains(x.FiscalYear))
                                 && (string.IsNullOrEmpty(sKUInput.Region.Trim()) || regionList.Contains(x.Region)))
                                 .ToListAsync();

                var records = list.Where(y => ((sKUInput.TotalRevenueMaxValue == 0) || ((decimal?)y.Revenue >= sKUInput.TotalRevenueMinValue
                                  && (decimal)y.Revenue <= sKUInput.TotalRevenueMaxValue))
                                  && ((sKUInput.SalesQuantityMaxValue == 0) || ((decimal?)y.Quantity >= sKUInput.SalesQuantityMinValue
                                  && (decimal)y.Quantity <= sKUInput.SalesQuantityMaxValue)))
                                  .OrderBy(x => x.SKU);
                #endregion

                #region Data_ToExport

                bMSRegulatorySKUExports = (from x in records
                                           select new BMSRegulatorySKUExport
                                           {
                                               SKU = x.SKU,
                                               Region = x.Region,
                                               SoldToCountry = x.SoldToCountry,
                                               ProductDescription = x.ProductDescription,
                                               Quantity = x.Quantity,
                                               Revenue = x.Revenue,
                                               ProfitCtrSBUName = x.ProfitCtrSBUName,
                                               SBUName = x.SBUName,
                                               FiscalYear = x.FiscalYear,
                                               LOBNameLineofBusiness = x.LOBNameLineofBusiness,
                                               OriginLocation = x.OriginLocation,
                                               OfferingManager = x.OfferingManager,
                                               LeadSupplyLocation = x.LeadSupplyLocation,
                                               PrdLnSubGrpSalesProductType = x.PrdLnSubGrpSalesProductType,
                                               ProdFamilyBrand = x.ProdFamilyBrand,
                                               ProdLineSalesProductCategory = x.ProdLineSalesProductCategory,
                                               ProfitCtrLOBNameReleaseTrain = x.ProfitCtrLOBNameReleaseTrain
                                           }).ToList();
                #endregion
            }
            catch (Exception e)
            {
                //logger.LogError(e.Message);
            }
            return bMSRegulatorySKUExports;
        }
    }
}
