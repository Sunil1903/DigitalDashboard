using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DigitalDashboard.DAL.Models
{
    public class BMSRegulatoryCountry
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; set; } = string.Empty;

        [BsonElement("Regulatory Category")]
        [BsonRepresentation(BsonType.String)]
        public string RegulatoryCategory { get; set; } = string.Empty;

        [BsonElement("Mandatory Certification Scheme")]
        [BsonRepresentation(BsonType.String)]
        public string MandatoryCertificationScheme { get; set; } = string.Empty;

        [BsonElement("Optional Certification Scheme")]
        [BsonRepresentation(BsonType.String)]
        public string OptionalCertificationScheme { get; set; } = string.Empty;

        [BsonElement("Testing Required?")]
        [BsonRepresentation(BsonType.String)]
        public string TestingRequired { get; set; } = string.Empty;

        [BsonElement("Customs Considerations")]
        [BsonRepresentation(BsonType.String)]
        public string CustomsConsiderations { get; set; } = string.Empty;

        [BsonElement("Marking Requirements")]
        [BsonRepresentation(BsonType.String)]
        public string MarkingRequirements { get; set; } = string.Empty;

        [BsonElement("Contact")]
        [BsonRepresentation(BsonType.String)]
        public string Contact { get; set; } = string.Empty;

        [BsonElement("Agency References")]
        [BsonRepresentation(BsonType.String)]
        public string AgencyReferences { get; set; } = string.Empty;

        [BsonElement("Notes")]
        [BsonRepresentation(BsonType.String)]
        public string Notes { get; set; } = string.Empty;

        [BsonElement("Details")]
        [BsonRepresentation(BsonType.String)]
        public string Details { get; set; } = string.Empty;

        [BsonElement("Region")]
        [BsonRepresentation(BsonType.String)]
        public string Region { get; set; } = string.Empty;

        [BsonElement("Country")]
        [BsonRepresentation(BsonType.String)]
        public string Country { get; set; } = string.Empty;

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
    }
}

