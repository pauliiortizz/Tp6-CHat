using Backend.Repositories;
using Backend.Services;
using EmployeeCrudApi.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Repositories
{
    public class MongoProductRepository : IProductRepository
    {
        private readonly IMongoCollection<Product> _collection;
        private readonly CounterService _counter;

        public MongoProductRepository(Backend.Services.MongoDbService mongo, CounterService counter)
        {
            _collection = mongo.GetCollection<Product>("products");
            _counter = counter;
            // Optional: indices pueden agregarse aquí si hace falta
            // var idx = Builders<Product>.IndexKeys.Ascending(p => p.Name);
            // _collection.Indexes.CreateOne(new CreateIndexModel<Product>(idx));
        }

        public async Task<List<Product>> GetAllAsync()
        {
            return await _collection.Find(Builders<Product>.Filter.Empty).ToListAsync();
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            var filter = Builders<Product>.Filter.Eq(p => p.Id, id);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<Product> CreateAsync(Product product)
        {
            if (product.Id == 0)
            {
                product.Id = await _counter.GetNextAsync("Product");
            }
            await _collection.InsertOneAsync(product);
            return product;
        }

        public async Task<Product> UpdateAsync(Product product)
        {
            var filter = Builders<Product>.Filter.Eq(p => p.Id, product.Id);
            var result = await _collection.ReplaceOneAsync(filter, product, new ReplaceOptions { IsUpsert = false });
            if (result.MatchedCount == 0) return null;
            return product;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var filter = Builders<Product>.Filter.Eq(p => p.Id, id);
            var result = await _collection.DeleteOneAsync(filter);
            return result.DeletedCount > 0;
        }
    }
}
