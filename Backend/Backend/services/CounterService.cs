using MongoDB.Driver;
using System.Threading.Tasks;

namespace Backend.Services
{
    public class CounterService
    {
        private readonly IMongoCollection<CounterDocument> _counters;

        public CounterService(MongoDbService mongo)
        {
            _counters = mongo.GetCollection<CounterDocument>("counters");
        }

        public async Task<int> GetNextAsync(string key)
        {
            var filter = Builders<CounterDocument>.Filter.Eq(x => x.Id, key);
            var update = Builders<CounterDocument>.Update.Inc(x => x.Seq, 1);
            var options = new FindOneAndUpdateOptions<CounterDocument>
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            };
            var result = await _counters.FindOneAndUpdateAsync(filter, update, options);
            return result.Seq;
        }

        public class CounterDocument
        {
            public string Id { get; set; } = null!; // Mongo _id = key
            public int Seq { get; set; }
        }
    }
}
