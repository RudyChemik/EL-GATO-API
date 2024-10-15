using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ElGato_API.ModelsMongo.Meal
{
    public class MealLikesDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string UserId { get; set; }
        public List<string> LikedMeals { get; set; } = new List<string>();
        public List<string> SavedMeals { get; set; } = new List<string>();
    }
}
