using ElGato_API.Models.User;
using MongoDB.Bson;

namespace ElGato_API.ModelsMongo.Diet
{
    public class DietDocument
    {
        public ObjectId Id { get; set; }
        public string UserId { get; set; }
        public List<DailyDietPlan> DailyPlans { get; set; } = new List<DailyDietPlan>();

    }
}
