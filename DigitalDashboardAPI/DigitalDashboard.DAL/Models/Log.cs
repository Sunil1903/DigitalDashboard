using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalDashboard.DAL.Models
{
    public class Log
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; set; } = string.Empty;

        [BsonElement("status")]
        public string Status { get; set; } = string.Empty;

        [BsonElement("logTime")]
        public DateTime LogTime { get; set; } = DateTime.Now;

        [BsonElement("message")]
        public string Message { get; set; } = string.Empty;

        [BsonElement("expireAt")]
        public DateTime ExpireAt { get; set; }
    }
}
