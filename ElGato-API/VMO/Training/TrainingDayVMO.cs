using ElGato_API.ModelsMongo.Training;

namespace ElGato_API.VMO.Training
{
    public class TrainingDayVMO
    {
        public DateTime Date { get; set; }
        public List<DailyExercise> Exercises { get; set; }
    }
}
