using DigitalDashboard.BLL.Interfaces;
using DigitalDashboard.DAL.DTO;
using DigitalDashboard.DAL.Models;
using DnsClient.Protocol;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalDashboard.BLL.Repository
{
    public class RegulationsByCountryRepository : IRegulationsByCountryRepository
    {
        private readonly IMongoCollection<BMSRegulatoryCountry> countryCollection;

        // Constructor:
        //    DigitalDashboardDatabaseSettings instance is retrieved from DI
        //    via constructor injection

        public RegulationsByCountryRepository(IDigitalDashboardDatabaseSettings settings,
                                              IMongoClient mongoClient)
        {
            var mongoDatabase = mongoClient.GetDatabase(settings.DatabaseName);
            countryCollection = mongoDatabase.GetCollection<BMSRegulatoryCountry>(settings.BMSRegulationsByCountryCollectionName);
        }

        public async Task<BMSRegulatoryCountryWithTotalCount> GetRegulationsByCountryAsync(BMSRegulatoryCountryInput countryInput,
                                                                                           int dataRange,
                                                                                           int skipRecordCount)
        {
            BMSRegulatoryCountryWithTotalCount bmsRegulatoryCountryWithTotalCount = new BMSRegulatoryCountryWithTotalCount();
            try
            {
                string[] regionList = countryInput.Region.Split(BaseClass.CharacterDelimiter);
                string[] CountryList = countryInput.Country.Split(BaseClass.CharacterDelimiter);
                string[] RegulatoryCategoryList = countryInput.RegulatoryCategory.Split(BaseClass.CharacterDelimiter);

                var result = await countryCollection
                                    .Find(x => x.Active.ToLower() == "true" 
                                    && (string.IsNullOrEmpty(countryInput.Region) 
                                        || regionList.Contains(x.Region)) 
                                    && (string.IsNullOrEmpty(countryInput.Country.Trim()) 
                                        || CountryList.Contains(x.Country)) 
                                    && (string.IsNullOrEmpty(countryInput.RegulatoryCategory.Trim()) 
                                        || RegulatoryCategoryList.Contains(x.RegulatoryCategory)))
                    .ToListAsync();
                //.OrderBy(x => x.Region);

                bmsRegulatoryCountryWithTotalCount.TotalCount = result.Count();
                bmsRegulatoryCountryWithTotalCount.bmsRegulatoryCountry = result.Skip(skipRecordCount)
                                                                                .Take(dataRange)
                                                                                .ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                //logger.LogError(e.Message);
            }
            return bmsRegulatoryCountryWithTotalCount;
        }
        
        // Summary:
        //    Get details of country regulations filtered by search input
        //
        // Parameters:
        //    searchText: User input to search or filter records
        //
        // Return:
        //    
        // 
        public async Task<BMSRegulatoryCountryWithTotalCount> SearchRegulationsByCountryUsingTextAsync(string searchText)
        {
            searchText = searchText.ToLower();
            
            List<BMSRegulatoryCountry> bmsRegulatoryCountry = new List<BMSRegulatoryCountry>();
            BMSRegulatoryCountryWithTotalCount bmsRegulatoryCountryWithTotalCount = new();
            
            var records = await countryCollection
                                .Find(x => x.Active == "true" && x.Country.ToLower().StartsWith(searchText) || x.Region.ToLower().StartsWith(searchText) || x.RegulatoryCategory.ToLower().StartsWith(searchText))
                                .ToListAsync();
            
            if (records.Count > 0)
            {
                bmsRegulatoryCountryWithTotalCount.TotalCount = records.Count();
                bmsRegulatoryCountryWithTotalCount.bmsRegulatoryCountry = records.ToList();
            }
            
            return bmsRegulatoryCountryWithTotalCount;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<RegulatoryCategoryList> GetFilteredRegulatoryCategoryAsync()
        {
            BMSRegulatorySKUFilterData bmsregulatorysku = new BMSRegulatorySKUFilterData();
            
            var list = await countryCollection
                                .Find(x => x.Active == "true")
                                .ToListAsync();

            var records = list.OrderBy(x => x.Region);

            RegulatoryCategoryList returnCollection = new RegulatoryCategoryList();

            RegionWithCountry regionWithCountry;
            RegulatoryCategoryWithCountry regulatoryCategoryWithCountry;

            returnCollection.regionwithcountry = new List<RegionWithCountry>();
            returnCollection.regulatorycategorywithcountry = new List<RegulatoryCategoryWithCountry>();

            foreach (var vr in records)
            {
                regionWithCountry = new RegionWithCountry();                
                regionWithCountry.Country = vr.Country;
                regionWithCountry.Region = vr.Region;
                returnCollection.regionwithcountry.Add(regionWithCountry);

                regulatoryCategoryWithCountry = new RegulatoryCategoryWithCountry();
                regulatoryCategoryWithCountry.Country = vr.Country;
                regulatoryCategoryWithCountry.Regulatory = vr.RegulatoryCategory;
                returnCollection.regulatorycategorywithcountry.Add(regulatoryCategoryWithCountry);
            }

            return returnCollection;
        }

        #region New Service Added On: 01 Dec 2022
        public async Task<RegulationByCountryDataWithFilters> GetRegulationsByCountryWithFiltersAsync(BMSRegulatoryCountryInput countryInput,
                                                                                                int dataRange,
                                                                                                int skipRecordCount)
        {
            RegulationByCountryDataWithFilters retrunCountryDataWithFilters = new RegulationByCountryDataWithFilters();

            try
            {
                // Splitting items with semicolon (;) and storing in an array
                string[] regionList = countryInput.Region.Split(BaseClass.CharacterDelimiter);
                string[] CountryList = countryInput.Country.Split(BaseClass.CharacterDelimiter);
                string[] RegulatoryCategoryList = countryInput.RegulatoryCategory.Split(BaseClass.CharacterDelimiter);
                
                #region Fetching_AllRecords
                var list = await countryCollection
                                .Find(x => x.Active.ToLower() == "true"
                                && (string.IsNullOrEmpty(countryInput.Region)
                                    || regionList.Contains(x.Region))
                                && (string.IsNullOrEmpty(countryInput.Country.Trim())
                                    || CountryList.Contains(x.Country))
                                && (string.IsNullOrEmpty(countryInput.RegulatoryCategory.Trim())
                                    || RegulatoryCategoryList.Contains(x.RegulatoryCategory)))
                                .ToListAsync();
                
                var records = list.OrderBy(x => x.Region);
                #endregion

                // Storing country data with total count
                #region Countries_DataWithTotalCount
                BMSRegulatoryCountryWithTotalCount countryWithTotalCount = new BMSRegulatoryCountryWithTotalCount();
                countryWithTotalCount.TotalCount = records.Count();
                countryWithTotalCount.bmsRegulatoryCountry = records.Skip(skipRecordCount)
                                                                    .Take(dataRange)
                                                                    .ToList();
                retrunCountryDataWithFilters.RegulatoryCountryWithTotalCount = countryWithTotalCount;
                #endregion

                // Storing list of countries filter
                #region Countries_Filter
                RegulationByCountryFilters regulationByCountryFilters = new RegulationByCountryFilters();
                regulationByCountryFilters.Countries = records.OrderBy(e => e.Country).Select(ex => ex.Country).Distinct().ToList();
                regulationByCountryFilters.RegulatoryCategories = records.OrderBy(e => e.RegulatoryCategory).Select(ex => ex.RegulatoryCategory).Distinct().ToList();
                regulationByCountryFilters.Regions = records.OrderBy(e => e.Region).Select(ex => ex.Region).Distinct().ToList();

                retrunCountryDataWithFilters.RegulationByCountryWithFilters = regulationByCountryFilters;
                #endregion
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                //logger.LogError(e.Message);
            }
            return retrunCountryDataWithFilters;
        }
        #endregion
    }
}
