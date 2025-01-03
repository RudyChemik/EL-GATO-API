﻿using ElGato_API.ModelsMongo.Training;
using MongoDB.Bson;

namespace ElGato_API.ModelsMongo.Diet.History
{
    public class TrainingHistoryDocument
    {
        public ObjectId Id { get; set; }
        public string UserId { get; set; }
        List<DailyTrainingPlan> DailyTrainingPlans { get; set; } = new List<DailyTrainingPlan>();
    }
}
