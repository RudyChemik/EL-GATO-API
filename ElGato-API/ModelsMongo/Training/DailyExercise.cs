namespace ElGato_API.ModelsMongo.Training
{
    public class DailyExercise
    {
        public string Name { get; set; }
        public List<ExerciseSeries> Series { get; set; }
    }

    public class ExerciseSeries
    {
        public int Id { get; set; }
        public int Repetitions { get; set; }
        public double Weight { get; set; }
        public ExerciseSerieTempo? Tempo { get; set; }
    }

    public class ExerciseSerieTempo
    {
        public double UpHold { get; set; }
        public double Up {  get; set; }
        public double Down {  get; set; }
        public double DownHold { get; set; }
    }
}
