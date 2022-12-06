namespace DigitalDashboard.DAL.Models
{
    public class BMSRegulatorySKUExport
    {
        public string SKU { get; set; } = string.Empty;
        public string ProductDescription { get; set; } = string.Empty;
        public string OfferingManager { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string SoldToCountry { get; set; } = string.Empty;
        public string FiscalYear { get; set; } = string.Empty;
        public string LeadSupplyLocation { get; set; } = string.Empty;
        public string ProfitCtrSBUName { get; set; } = string.Empty;
        public string SBUName { get; set; } = string.Empty;
        public string OriginLocation { get; set; } = string.Empty;
        public string ProfitCtrLOBNameReleaseTrain { get; set; } = string.Empty;
        public string LOBNameLineofBusiness { get; set; } = string.Empty;
        public string ProdFamilyBrand { get; set; } = string.Empty;
        public string ProdLineSalesProductCategory { get; set; } = string.Empty;
        public string PrdLnSubGrpSalesProductType { get; set; } = string.Empty;
        public double? Quantity { get; set; }
        public double? Revenue { get; set; }
    }
}

