using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Backend.Services
{
    public class MongoDbService
    {
        private readonly IMongoDatabase _database;

        public MongoDbService(IConfiguration config, ILogger<MongoDbService> logger)
        {
            var connectionString = config.GetConnectionString("MongoDb");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("MongoDb connection string is not configured.");
            }

            // Log sanitized raw connection string to help diagnose placeholder/override issues
            try
            {
                var raw = connectionString;
                string sanitizedRaw = raw;
                if (raw.Contains("@") && raw.Contains("://"))
                {
                    // basic remove of credentials between :// and @
                    var parts = raw.Split(new[] {"://"}, 2, StringSplitOptions.None);
                    if (parts.Length == 2)
                    {
                        var scheme = parts[0];
                        var rest = parts[1];
                        var atIndex = rest.IndexOf('@');
                        if (atIndex > 0)
                        {
                            var after = rest.Substring(atIndex + 1);
                            sanitizedRaw = scheme + "://<redacted>@" + after;
                        }
                    }
                }
                logger.LogInformation("MongoDb connection string (sanitized): {cs}", sanitizedRaw);
            }
            catch (Exception logEx)
            {
                logger.LogWarning(logEx, "Failed to sanitize/log MongoDb connection string");
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

            // Safe logging: show the hosts (no credentials) so we can diagnose DNS/host issues.
            var mongoUrl = MongoUrl.Create(connectionString);
            var hosts = string.Join(',', mongoUrl.Servers.Select(s => s.Host));
            // Sanitize URL for logging: remove username/password if present
            var sanitized = mongoUrl.ToString();
            if (!string.IsNullOrEmpty(mongoUrl.Username))
            {
                // replace credentials portion user:pass@ with <redacted>@
                var builder = sanitized;
                var creds = mongoUrl.Username + ":" + (mongoUrl.Password ?? string.Empty) + "@";
                sanitized = builder.Replace(creds, "<redacted>@");
            }
            logger.LogInformation("MongoDB hosts: {hosts}. Using database: {db}. Url: {url}", hosts, configuredDatabase, sanitized);

            try
            {
                var mongoClient = new MongoClient(connectionString);
                _database = mongoClient.GetDatabase(configuredDatabase);
            }
            catch (Exception ex)
            {
                // Log with context and rethrow so startup shows a clearer error in logs.
                logger.LogError(ex, "Failed to create MongoClient. Check connection string and DNS for hosts: {hosts}", hosts);
                throw;
            }
        }

        public IMongoCollection<T> GetCollection<T>(string name)
        {
            return _database.GetCollection<T>(name);
        }
    }
}
