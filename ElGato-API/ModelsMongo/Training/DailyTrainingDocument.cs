using ElGato_API.ModelsMongo.Diet;
using MongoDB.Bson;

namespace ElGato_API.ModelsMongo.Training
{
    public class DailyTrainingDocument
    {
        public ObjectId Id { get; set; }
        public string UserId { get; set; }
        public List<DailyTrainingPlan> Trainings { get; set; } = new List<DailyTrainingPlan>();
    }
}
