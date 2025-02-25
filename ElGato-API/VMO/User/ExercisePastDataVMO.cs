namespace ElGato_API.VMO.User
{
    public class ExercisePastDataVMO
    {
        public string ExerciseName { get; set; }
        public List<ExercisePastData> PastData { get; set; } = new List<ExercisePastData>();
    }

    public class ExercisePastData
    {
        public DateTime Date { get; set; }
        public List<ExercisePastSerieData> Series { get; set; } = new List<ExercisePastSerieData>();
    }

    public class ExercisePastSerieData
    {
        public double WeightKg { get; set; }
        public double WeightLbs { get; set; }
        public int Repetitions { get; set; }
    }
}
