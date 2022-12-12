using DigitalDashboard.DAL.DTO;
using DigitalDashboard.DAL.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalDashboard.BLL.Interfaces
{
    public interface IRegulationsByCountryRepository
    {
        Task<BMSRegulatoryCountryWithTotalCount> GetRegulationsByCountryAsync(BMSRegulatoryCountryInput countryInput,
                                                                              int dataRange, 
                                                                              int skipRecordCount);
        // New Service Added On: 01 Dec 2022
        Task<RegulationByCountryDataWithFilters> GetRegulationsByCountryWithFiltersAsync(BMSRegulatoryCountryInput countryInput,
                                                                             int dataRange,
                                                                             int skipRecordCount);

        Task<BMSRegulatoryCountryWithTotalCount> SearchRegulationsByCountryUsingTextAsync(string searchText);
        
        Task<RegulatoryCategoryList> GetFilteredRegulatoryCategoryAsync();
    }
}
