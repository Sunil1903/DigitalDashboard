using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalDashboard.DAL.DTO
{
    public class RegulationByCountryFilters
    {
        public List<string> Countries { get; set; }
        public List<string> RegulatoryCategories { get; set; }
        public List<string> Regions { get; set; }
    }
}
