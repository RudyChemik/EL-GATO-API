using ElGato_API.ModelsMongo.Training;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ElGato_API.ModelsMongo.History
{
    public class ExercisesHistoryDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string UserId { get; set; }
        public List<ExerciseHistoryList> ExerciseHistoryLists {  get; set; } = new List<ExerciseHistoryList>();
    }

    public class ExerciseHistoryList
    {
        public string ExerciseName { get; set; }
        public MuscleType MuscleType { get; set; } = MuscleType.Unknown;
        public List<ExerciseData> ExerciseData { get; set; } = new List<ExerciseData>();
    }

    public class ExerciseData
    {
        public DateTime Date { get; set; }
        public List<ExerciseSeries> Series { get; set; }
    }

    public enum MuscleType
    {
        Unknown,
        Chest,
        Legs,
        Back,
        Arms,
        Core,
        Shoulders
    }
}
