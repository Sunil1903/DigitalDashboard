namespace DigitalDashboard.DAL.Models
{
    public class BMSRegulatorySKUWithTotalCount
    {
        public List<BMSRegulatorySKU> bmsRegulatorySKUList { get; set; }
        public int TotalCount { get; set; }
        public decimal? GrandTotalOfTotalRevenue { get; set; }
        public decimal? GrandTotalOfSalesQuantity { get; set; }
    }
}

