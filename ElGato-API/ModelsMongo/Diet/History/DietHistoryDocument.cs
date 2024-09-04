using MongoDB.Bson;

namespace ElGato_API.ModelsMongo.Diet.History
{
    public class DietHistoryDocument
    {
        public ObjectId Id { get; set; }
        public string UserId { get; set; }
        public List<DailyDietPlan> DailyPlans { get; set; } = new List<DailyDietPlan>();
    }
}
