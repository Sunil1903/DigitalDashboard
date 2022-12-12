using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalDashboard.DAL.DTO
{
    public class RegulatorySKUFiltersDto
    {
        public List<string>? SKUList { get; set; }
        
        public List<string>? ProductDescription { get; set; }
        
        public List<string>? OfferingManager { get; set; }
        
        public List<string>? Region { get; set; }
        
        public List<string>? SoldToCountry { get; set; }
        
        public List<string>? FiscalYear { get; set; }
        
        public List<decimal>? Quantity { get; set; }
        
        public List<decimal>? Revenue { get; set; }
        
        public List<RegionWithSoldToCountry>? RegionWithSoldToCountry { get; set; }
    }
}
