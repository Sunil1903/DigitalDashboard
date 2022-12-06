using DigitalDashboard.BLL.Authorization;
using DigitalDashboard.BLL.Interfaces;
using DigitalDashboard.DAL.Models;
using DnsClient.Protocol;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalDashboard.BLL.Repository
{
    public class RegulatorySKURepository : IRegulatorySKURepository
    {
        private readonly IMongoCollection<BMSRegulatorySKU> sKUCollection;
        private readonly IUserAuthorization authorization;

        // Constructor:
        //    DigitalDashboardDatabaseSettings instance is retrieved from DI
        //    via constructor injection

        public RegulatorySKURepository(IDigitalDashboardDatabaseSettings settings,
                                       IMongoClient mongoClient,
                                       IUserAuthorization authorization)
        {
            var mongoDatabase = mongoClient.GetDatabase(settings.DatabaseName);
            sKUCollection = mongoDatabase.GetCollection<BMSRegulatorySKU>(settings.BMSRegulatorySKUCollectionName);
            this.authorization = authorization;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<BMSRegulatorySKUFilterData> GetFilteredRegulatorySKUAsync()
        {
            bool isAuthorized = authorization.VerifyAccessRights();

            BMSRegulatorySKUFilterData returnCollection = new BMSRegulatorySKUFilterData();

            var result = await sKUCollection
                               .Find(bms => bms.Active == "true")
                               .ToListAsync();

            if (result.Count > 0)
            {
                returnCollection.OfferingManager = result.Select(ex => ex.OfferingManager).Distinct().ToList();
                returnCollection.Region = result.Select(ex => ex.Region).Distinct().ToList();
                returnCollection.SoldToCountry = result.Select(ex => ex.SoldToCountry).Distinct().ToList();
                returnCollection.FiscalYear = result.Select(ex => ex.FiscalYear).Distinct().ToList();
                returnCollection.Quantity = result.Select(ex => Convert.ToDecimal(ex.Quantity)).Distinct().ToList();
                returnCollection.Revenue = result.Select(ex => Convert.ToDecimal(ex.Revenue)).Distinct().ToList();

                if (isAuthorized)
                {
                    returnCollection.Quantity = result.Select(ex => Convert.ToDecimal(ex.Quantity)).Distinct().ToList();
                    returnCollection.Revenue = result.Select(ex => Convert.ToDecimal(ex.Revenue)).Distinct().ToList();
                }
                else
                {
                    returnCollection.Quantity = null;
                    returnCollection.Revenue = null;
                }
            }
            return returnCollection;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sKUInput"></param>
        /// <param name="dataRange"></param>
        /// <param name="skipRecordCount"></param>
        /// <returns></returns>
        public async Task<RegulatorySKUDataWithFilterList> GetRegulatorySKUWithFilterListAsync(BMSRegulatorySKUInput sKUInput, 
                                                                                                   int dataRange, 
                                                                                                   int skipRecordCount)
        {
            bool isAuthorized = authorization.VerifyAccessRights();
            RegulatorySKUDataWithFilterList returnCollection = new RegulatorySKUDataWithFilterList();
            BMSRegulatorySKUFilterData bmsregulatorysku = new BMSRegulatorySKUFilterData();
            try
            {
                // List<BMSRegulatorySKU> bmsregulatory = new List<BMSRegulatorySKU>();
                BMSRegulatorySKUWithTotalCount bmsregulatoryskuwithtotalcount = new BMSRegulatorySKUWithTotalCount();

                #region FilteringInputParameters
                
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
                                && (string.IsNullOrEmpty(sKUInput.Region.Trim()) || regionList.Contains(x.Region))).ToListAsync();

                var records = list.Where(y => ((sKUInput.TotalRevenueMaxValue == 0) || ((decimal?)y.Revenue >= sKUInput.TotalRevenueMinValue
                                   && (decimal)y.Revenue <= sKUInput.TotalRevenueMaxValue))
                                   && ((sKUInput.SalesQuantityMaxValue == 0) || ((decimal?)y.Quantity >= sKUInput.SalesQuantityMinValue
                                   && (decimal)y.Quantity <= sKUInput.SalesQuantityMaxValue)))
                                   .OrderBy(x => x.SKU);
                #endregion 

                #region Assigning_DataToObjects

                bmsregulatoryskuwithtotalcount.TotalCount = records.Count();
                bmsregulatoryskuwithtotalcount.bmsRegulatorySKUList = records.Skip(skipRecordCount).Take(dataRange).ToList();
                bmsregulatoryskuwithtotalcount.TotalCount = records.Count();
                bmsregulatoryskuwithtotalcount.bmsRegulatorySKUList = records.Skip(skipRecordCount).Take(dataRange).ToList();

                bmsregulatoryskuwithtotalcount.GrandTotalOfSalesQuantity = records.Sum(x => (decimal?)x.Quantity);
                bmsregulatoryskuwithtotalcount.GrandTotalOfTotalRevenue = records.Sum(x => (decimal?)x.Revenue);

                bmsregulatorysku.SKUList = records.OrderBy(x => x.SKU).Select(ex => ex.SKU).Distinct().Take(100).ToList();
                bmsregulatorysku.OfferingManager = records.OrderBy(e => e.OfferingManager).Select(ex => ex.OfferingManager).Distinct().ToList();
                bmsregulatorysku.Region = records.OrderBy(e => e.Region).Select(ex => ex.Region).Distinct().ToList();
                bmsregulatorysku.SoldToCountry = records.OrderBy(e => e.SoldToCountry).Select(ex => ex.SoldToCountry).Distinct().ToList();
                bmsregulatorysku.FiscalYear = records.OrderBy(e => e.FiscalYear).Select(ex => ex.FiscalYear).Distinct().ToList();

                if (isAuthorized)
                {
                    bmsregulatoryskuwithtotalcount.GrandTotalOfSalesQuantity = records.Sum(x => (decimal?)x.Quantity);
                    bmsregulatoryskuwithtotalcount.GrandTotalOfTotalRevenue = records.Sum(x => (decimal?)x.Revenue);
                    bmsregulatorysku.Quantity = records.OrderBy(e => e.Quantity).Select(ex => Convert.ToDecimal(ex.Quantity)).Distinct().ToList();
                    bmsregulatorysku.Revenue = records.OrderBy(e => e.Revenue).Select(ex => Convert.ToDecimal(ex.Revenue)).Distinct().ToList();
                }
                else
                {
                    bmsregulatoryskuwithtotalcount.bmsRegulatorySKUList.ForEach(x =>
                    {
                        x.Revenue = null;
                        x.Quantity = null;
                    });

                    bmsregulatoryskuwithtotalcount.GrandTotalOfSalesQuantity = null; /// AllData.Sum(x => (decimal)x.Quantity);
                    bmsregulatoryskuwithtotalcount.GrandTotalOfTotalRevenue = null;//AllData.Sum(x => (decimal)x.Revenue);
                    bmsregulatorysku.Quantity = null;//AllData.OrderBy(e => e.Quantity).Select(ex => Convert.ToDecimal(ex.Quantity)).Distinct().ToList();
                    bmsregulatorysku.Revenue = null;//AllData.OrderBy(e => e.Revenue).Select(ex => Convert.ToDecimal(ex.Revenue)).Distinct().ToList();
                }

                returnCollection.bmsRegulatorySKUWithTotalCount = bmsregulatoryskuwithtotalcount;
                returnCollection.bmsRegulatorySKUFilterData = bmsregulatorysku;

                #endregion 
            }
            catch (Exception e)
            {
                //logger.LogError(e.Message);
            }
            return returnCollection;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sKUInput"></param>
        /// <param name="dataRange"></param>
        /// <param name="skipRecordCount"></param>
        /// <returns></returns>
        public async Task<BMSRegulatorySKUWithTotalCount> GetSKUWithTotalCountAsync(BMSRegulatorySKUInput sKUInput,
                                                                                int dataRange,
                                                                                int skipRecordCount)
        {
            bool isAuthorized = authorization.VerifyAccessRights();
            BMSRegulatorySKUWithTotalCount returnCollection = new BMSRegulatorySKUWithTotalCount();
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
                                && (string.IsNullOrEmpty(sKUInput.Region.Trim()) || regionList.Contains(x.Region))).ToListAsync();

                var records = list.Where(y => ((sKUInput.TotalRevenueMaxValue == 0) || ((decimal?)y.Revenue >= sKUInput.TotalRevenueMinValue
                                && (decimal)y.Revenue <= sKUInput.TotalRevenueMaxValue))
                                && ((sKUInput.SalesQuantityMaxValue == 0) || ((decimal?)y.Quantity >= sKUInput.SalesQuantityMinValue
                                && (decimal)y.Quantity <= sKUInput.SalesQuantityMaxValue)))
                                .OrderBy(x => x.SKU);

                #endregion

                #region Setup_ReturnCollection
                
                returnCollection.TotalCount = records.Count();
                returnCollection.bmsRegulatorySKUList = records.Skip(skipRecordCount).Take(dataRange).ToList();
                if (isAuthorized)
                {
                    returnCollection.GrandTotalOfSalesQuantity = records.Sum(x => (decimal?)x.Quantity);
                    returnCollection.GrandTotalOfTotalRevenue = records.Sum(x => (decimal?)x.Revenue);
                }
                else
                {
                    returnCollection.bmsRegulatorySKUList.Select(c => { c.Quantity = null; return c; }).ToList();
                    returnCollection.bmsRegulatorySKUList.Select(c => { c.Revenue = null; return c; }).ToList();
                    returnCollection.GrandTotalOfSalesQuantity = null; /// AllData.Sum(x => (decimal)x.Quantity);
                    returnCollection.GrandTotalOfTotalRevenue = null;//AllData.Sum(x => (decimal)x.Revenue);
                }
                #endregion
            }
            catch (Exception e)
            {
                //logger.LogError(e.Message);
            }
            return returnCollection;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="dataRange"></param>
        /// <param name="skipRecordCount"></param>
        /// <returns></returns>
        public async Task<ColumnSKUList> GetSKUSearchListAsync(string searchText,
                                                            int dataRange,
                                                            int skipRecordCount)
        {
            searchText = searchText.Trim().ToLower();
            ColumnSKUList columnSKUList = new ColumnSKUList();

            if (string.IsNullOrEmpty(searchText))
            {
                var records = await sKUCollection
                                    .Find(x => x.Active == "true")
                                    .ToListAsync();

                columnSKUList.TotalCount = records.Select(x => x.SKU).Distinct().Count();
                columnSKUList.SKUList = records.Select(x => x.SKU).Distinct()
                                            .Skip(skipRecordCount)
                                            .Take(dataRange == 0 ? 100 : dataRange)
                                            .Distinct()
                                            .ToList();
            }
            else
            {
                var records = await sKUCollection
                                    .Find(x => x.Active == "true" && x.SKU.ToLower().StartsWith(searchText))
                                    .ToListAsync();

                columnSKUList.TotalCount = records.Select(x => x.SKU).Distinct().Count();
                columnSKUList.SKUList = records.Select(x => x.SKU)
                                            .Distinct()
                                            .Skip(skipRecordCount)
                                            .Take(dataRange)
                                            .ToList();
            }
            return columnSKUList;
        }        
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sKUInput"></param>
        /// <param name="dataRange"></param>
        /// <param name="skipRecordCount"></param>
        /// <returns></returns>
        public async Task<ColumnSKUList> GetSKUSearchListByCascadingFilterAsync(BMSRegulatorySKUInput sKUInput,
                                                                       int dataRange,
                                                                       int skipRecordCount)
        {
            ColumnSKUList columnSKUList = new ColumnSKUList();
            try
            {
                int totalCount = 0;
                var list = await GetBMSRegulatorySKUAsync(sKUInput);

                totalCount = list.Count();

                var records = list.Where(y => ((sKUInput.TotalRevenueMaxValue == 0) || ((decimal?)y.Revenue >= sKUInput.TotalRevenueMinValue 
                                  && (decimal)y.Revenue <= sKUInput.TotalRevenueMaxValue)) 
                                  && ((sKUInput.SalesQuantityMaxValue == 0) || ((decimal?)y.Quantity >= sKUInput.SalesQuantityMinValue 
                                  && (decimal)y.Quantity <= sKUInput.SalesQuantityMaxValue)))
                                  .Skip(skipRecordCount)
                                  .Take(dataRange == 0 ? list.Count() : dataRange)
                                  .OrderBy(x => x.SKU)
                                  .Select(x => x.SKU)
                                  .Distinct()
                                  .ToList();

                columnSKUList.SKUList = records;
                columnSKUList.TotalCount = records.Count();                
            }
            catch (Exception e)
            {
               //logger.LogError(e.Message);
                //return StatusCodes.Status500InternalServerError;
            }
            return columnSKUList;
        }
        private async Task<List<BMSRegulatorySKU>> GetBMSRegulatorySKUAsync(BMSRegulatorySKUInput sKUInput)
        {
            List<BMSRegulatorySKU> bMSRegulatorySKUList = new List<BMSRegulatorySKU>();
            
            try
            {
                #region Input_Parameters
                string?[] regionList = { null };
                string?[] SKUList = { null };
                string?[] ProductDescriptionList = { null };
                string?[] OfferingManagerList = { null };
                string?[] SoldToCountryList = { null };
                string?[] FiscalYearList = { null };
                string?[] idList = { null };

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

                #region Fetching_Data

                bMSRegulatorySKUList = await sKUCollection
                                            .Find(x => x.Active.ToLower() == "true" 
                                            && (string.IsNullOrEmpty(sKUInput.ID) || idList.Contains(x.id)) 
                                            && (string.IsNullOrEmpty(sKUInput.SKU) || SKUList.Contains(x.SKU))
                                            && (string.IsNullOrEmpty(sKUInput.ProductDescription.Trim()) || ProductDescriptionList.Contains(x.ProductDescription)) 
                                            && (string.IsNullOrEmpty(sKUInput.OfferingManager.Trim()) || OfferingManagerList.Contains(x.OfferingManager)) 
                                            && (string.IsNullOrEmpty(sKUInput.SoldToCountry.Trim()) || SoldToCountryList.Contains(x.SoldToCountry)) 
                                            && (string.IsNullOrEmpty(sKUInput.FiscalYear.Trim()) || FiscalYearList.Contains(x.FiscalYear)) 
                                            && (string.IsNullOrEmpty(sKUInput.Region.Trim()) || regionList.Contains(x.Region)) 
                                            && (string.IsNullOrEmpty(sKUInput.strSearchText.Trim()) || x.SKU.ToLower().StartsWith(sKUInput.strSearchText.Trim().ToLower())))
                                            .ToListAsync();

                #endregion
            }
            catch (Exception e)
            {
                //logger.LogError(e.Message);
            }
            return bMSRegulatorySKUList;
        }
    }
}
