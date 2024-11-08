using ElGato_API.ModelsMongo.Diet;
using MongoDB.Bson;

namespace ElGato_API.ModelsMongo.Meal
{
    public class OwnMealsDocument
    {
        public ObjectId Id { get; set; }
        public string UserId { get; set; }
        public List<string> OwnMealsId { get; set; } = new List<string>();
        public List<MealPlan> SavedIngMeals { get; set; } = new List<MealPlan>();
    }
}
