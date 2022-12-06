namespace DigitalDashboard.DAL.Models
{
    public class BMSRegulatorySKUInput
    {
        public string ID { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string ProductDescription { get; set; } = string.Empty;
        public string OfferingManager { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string SoldToCountry { get; set; } = string.Empty;
        public string FiscalYear { get; set; } = string.Empty;
        public decimal SalesQuantityMinValue { get; set; }
        public decimal SalesQuantityMaxValue { get; set; }
        public decimal TotalRevenueMinValue { get; set; }
        public decimal TotalRevenueMaxValue { get; set; }
        public string strSearchText { get; set; }
    }
}

