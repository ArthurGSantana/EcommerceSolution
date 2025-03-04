using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace HubMinified.Data.Mongo.Context
{
    public class MongoContext
    {
        private readonly IMongoDatabase _mongoDb;
        public IMongoDatabase MongoDb => _mongoDb;
        private readonly MongoClient _client;
        public MongoClient Cliente => _client;

        public MongoContext(string mongoConnectionString)
        {
            _client = new MongoClient(mongoConnectionString);
            _mongoDb = _client.GetDatabase("db_teste");
        }
    }
}