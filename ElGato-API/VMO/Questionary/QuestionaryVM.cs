namespace ElGato_API.VMO.Questionary
{
    public class QuestionaryVM
    {
        public string? Name { get; set; }
        public short Age { get; set; }
        public double Weight { get; set; }
        public double Height { get; set; }
        public bool Woman { get; set; }
        public int Goal { get; set; } // 0-> utrzymać //1-> redukcja //2-> masiwo
        public int? BodyType { get; set; } //1 -> ekto //2-> endo //mezo zapisz jak 0
        public int Sleep { get; set; } //0 -> 8+ //1 -> 7-8 //2 -> 6-7 //3 -> <6
        public int TrainingDays { get; set; } // 0-7 w tyg
        public int DailyTimeSpendWorking { get; set; } //0-4h+
        public int JobType { get; set; } //0-> none, sitting //1-> moderate heavy //2-> extra heavy
        public bool Metric { get; set; } = true;
    }
}
