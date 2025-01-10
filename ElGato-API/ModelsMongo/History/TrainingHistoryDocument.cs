using ElGato_API.ModelsMongo.Training;
using MongoDB.Bson;

namespace ElGato_API.ModelsMongo.History
{
    public class TrainingHistoryDocument
    {
        public ObjectId Id { get; set; }
        public string UserId { get; set; }
        public List<DailyTrainingPlan> DailyTrainingPlans { get; set; } = new List<DailyTrainingPlan>();
    }
}
