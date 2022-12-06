namespace DigitalDashboard.DAL.Models
{
    public class BMSRegulatorySKUFilterData
    {
        public List<string> SKUList { get; set; }
        public List<string> ProductDescription { get; set; }
        public List<string> OfferingManager { get; set; }
        public List<string> Region { get; set; }
        public List<string> SoldToCountry { get; set; }
        public List<string> FiscalYear { get; set; }
        public List<decimal>? Quantity { get; set; }
        public List<decimal>? Revenue { get; set; }
    }
}

