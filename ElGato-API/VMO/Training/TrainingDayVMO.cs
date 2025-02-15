using ElGato_API.ModelsMongo.Training;

namespace ElGato_API.VMO.Training
{
    public class TrainingDayVMO
    {
        public DateTime Date { get; set; } 
        public List<TrainingDayExerciseVMO> Exercises { get; set; }
    }

    public class TrainingDayExerciseVMO
    {
        public DailyExercise Exercise { get; set; }
        public PastExerciseData? PastData { get; set; }
    }

    public class PastExerciseData
    {
        public DateTime Date { get; set; }
        public List<ExerciseSeries> Series { get; set; }
    }
}
