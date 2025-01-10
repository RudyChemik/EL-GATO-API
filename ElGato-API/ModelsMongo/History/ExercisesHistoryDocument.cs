using ElGato_API.ModelsMongo.Training;

namespace ElGato_API.ModelsMongo.History
{
    public class ExercisesHistoryDocument
    {
        public string UserId { get; set; }
        public List<ExerciseHistoryList> ExerciseHistoryLists {  get; set; } = new List<ExerciseHistoryList>();
    }

    public class ExerciseHistoryList
    {
        public string ExerciseName { get; set; }
        public List<ExerciseData> ExerciseData { get; set; } = new List<ExerciseData>();
    }

    public class ExerciseData
    {
        public DateTime Date { get; set; }
        public List<ExerciseSeries> Series { get; set; }
    }
}
