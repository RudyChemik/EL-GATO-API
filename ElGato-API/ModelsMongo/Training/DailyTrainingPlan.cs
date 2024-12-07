namespace ElGato_API.ModelsMongo.Training
{
    public class DailyTrainingPlan
    {
        public DateTime Date { get; set; }
        public List<DailyExercise> Exercises { get; set; } = new List<DailyExercise>();
    }
}
