using MongoDB.Driver;
using Microsoft.Extensions.Configuration;

namespace Backend.Services
{
    public class MongoDbService
    {
        private readonly IMongoDatabase _database;

        public MongoDbService(IConfiguration config)
        {
            var connectionString = config.GetConnectionString("MongoDb");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("MongoDb connection string is not configured.");
            }

            // Prefer explicit database name from configuration; fall back to the name embedded in the URI.
            var configuredDatabase = config["MongoDbSettings:DatabaseName"];
            if (string.IsNullOrWhiteSpace(configuredDatabase))
            {
                configuredDatabase = MongoUrl.Create(connectionString).DatabaseName;
            }

            if (string.IsNullOrWhiteSpace(configuredDatabase))
            {
                throw new InvalidOperationException("MongoDb database name is not configured.");
            }

            var mongoClient = new MongoClient(connectionString);
            _database = mongoClient.GetDatabase(configuredDatabase);
        }

        public IMongoCollection<T> GetCollection<T>(string name)
        {
            return _database.GetCollection<T>(name);
        }
    }
}
