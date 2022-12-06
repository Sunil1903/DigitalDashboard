using DigitalDashboard.DAL.Models;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DigitalDashboard.JobManager.Repository
{
    public class LogRepository : ILogRepository
    {
        private readonly IMongoCollection<Log> log;

        public LogRepository(IDigitalDashboardDatabaseSettings settings,
                             IMongoClient mongoClient)
        {
            var mongoDatabase = mongoClient.GetDatabase(settings.DatabaseName);
            log = mongoDatabase.GetCollection<Log>(settings.LogCollectionName);
        }

        public async Task<List<Log>> GetAllLog()
        {
            return await log.Find(_ => true).SortByDescending(x=> x.LogTime).ToListAsync();
        }

        public async Task Logging(Log newLog)
        {
            await log.Indexes.CreateOneAsync(new CreateIndexModel<Log>(new BsonDocument("expireAt", 1), new CreateIndexOptions { ExpireAfter = new TimeSpan(0, 0, 5) }));
            await log.InsertOneAsync(newLog);
        }
    }
}
