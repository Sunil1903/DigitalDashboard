using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalDashboard.DAL.Models
{
    [BsonIgnoreExtraElements]
    public class BMSRegulatorySKU
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; set; } = string.Empty;

        [BsonElement("SKU")]
        [BsonRepresentation(BsonType.String)]
        public string SKU { get; set; } = string.Empty;

        [BsonElement("Product Description")]
        [BsonRepresentation(BsonType.String)]
        public string ProductDescription { get; set; } = string.Empty;

        [BsonElement("Offering Manager")]
        [BsonRepresentation(BsonType.String)]
        public string OfferingManager { get; set; } = string.Empty;

        [BsonElement("Region")]
        [BsonRepresentation(BsonType.String)]
        public string Region { get; set; } = string.Empty;

        [BsonElement("Sold-To Country (TRX) Name")]
        [BsonRepresentation(BsonType.String)]
        public string SoldToCountry { get; set; } = string.Empty;

        [BsonElement("Fiscal Year")]
        [BsonRepresentation(BsonType.String)]
        public string FiscalYear { get; set; } = string.Empty;

        [BsonElement("Lead Supply Location")]
        [BsonRepresentation(BsonType.String)]
        public string LeadSupplyLocation { get; set; } = string.Empty;

        [BsonElement("Profit Ctr SBU Name")]
        [BsonRepresentation(BsonType.String)]
        public string ProfitCtrSBUName { get; set; } = string.Empty;

        [BsonElement("SBU Name")]
        [BsonRepresentation(BsonType.String)]
        public string SBUName { get; set; } = string.Empty;

        [BsonElement("Origin Location")]
        [BsonRepresentation(BsonType.String)]
        public string OriginLocation { get; set; } = string.Empty;

        [BsonElement("Profit Ctr LOB Name")]
        [BsonRepresentation(BsonType.String)]
        public string ProfitCtrLOBNameReleaseTrain { get; set; } = string.Empty;

        [BsonElement("LOB Name")]
        [BsonRepresentation(BsonType.String)]
        public string LOBNameLineofBusiness { get; set; } = string.Empty;

        [BsonElement("Prod Family (Sales) Name")]
        [BsonRepresentation(BsonType.String)]
        public string ProdFamilyBrand { get; set; } = string.Empty;

        [BsonElement("Prod Line (Sales) Name")]
        [BsonRepresentation(BsonType.String)]
        public string ProdLineSalesProductCategory { get; set; } = string.Empty;

        [BsonElement("PrdLn SubGrp (Sales) Name")]
        [BsonRepresentation(BsonType.String)]
        public string PrdLnSubGrpSalesProductType { get; set; } = string.Empty;

        [BsonElement("Active")]
        [BsonRepresentation(BsonType.String)]
        public string Active { get; set; } = string.Empty;

        [BsonElement("UpdateDatetime")]
        [BsonRepresentation(BsonType.String)]
        public string UpdateDatetime { get; set; } = string.Empty;

        [BsonElement("UserID")]
        [BsonRepresentation(BsonType.String)]
        public string UserID { get; set; } = string.Empty;

        [BsonElement("Status")]
        [BsonRepresentation(BsonType.String)]
        public string Status { get; set; } = string.Empty;

        [BsonElement("Quantity")]
        [BsonRepresentation(BsonType.Double)]
        public double? Quantity { get; set; }

        [BsonElement("Revenue")]
        [BsonRepresentation(BsonType.Double)]
        public double? Revenue { get; set; }
    }
}

