using ElGato_API.Models.User;
using ElGato_API.ModelsMongo.Diet;
using MongoDB.Driver;

namespace ElGato_API.Data
{
    public class MongoInits : IMongoInits
    {
        private readonly IMongoCollection<DietDocument> _dietCollection;
        public MongoInits(IMongoDatabase database) 
        {
            _dietCollection = database.GetCollection<DietDocument>("DailyDiet");
        }
        public async Task CreateUserDietDocument(string userId) 
        {
            var existingDocument = await _dietCollection.Find(d => d.UserId == userId).FirstOrDefaultAsync();
            if (existingDocument == null) 
            {
                var newDoc = new DietDocument { UserId = userId, DailyPlans = new List<DailyDietPlan>() };
                await _dietCollection.InsertOneAsync(newDoc);
            }
        }
    }
}
