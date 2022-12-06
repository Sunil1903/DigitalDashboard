namespace DigitalDashboard.DAL.Models
{
    public class BMSRegulatoryCountryWithTotalCount
    {
        public List<BMSRegulatoryCountry> bmsRegulatoryCountry { get; set; } = new List<BMSRegulatoryCountry>();
        public int TotalCount { get; set; }
    }
}

