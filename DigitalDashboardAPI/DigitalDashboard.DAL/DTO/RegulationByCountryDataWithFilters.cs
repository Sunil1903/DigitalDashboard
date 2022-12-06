using DigitalDashboard.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalDashboard.DAL.DTO
{
    public class RegulationByCountryDataWithFilters
    {
        public BMSRegulatoryCountryWithTotalCount RegulatoryCountryWithTotalCount { get; set; }
        public RegulationByCountryFilters RegulationByCountryWithFilters { get; set; }
    }
}
