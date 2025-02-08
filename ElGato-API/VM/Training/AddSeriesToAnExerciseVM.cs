using ElGato_API.ModelsMongo.History;
using System.ComponentModel.DataAnnotations;

namespace ElGato_API.VM.Training
{
    public class AddSeriesToAnExerciseVM
    {
        [Required(ErrorMessage = "Date is required")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Public id for an exercise you want to add series to is required.")]
        public int PublicId { get; set; }

        [Required(ErrorMessage = "Whole exercise data from current training day is required")]
        public HistoryUpdateVM HistoryUpdate { get; set; }
        public List<AddSeriesVM> Series { get; set; } = new List<AddSeriesVM>();
    }

    public class AddSeriesVM
    {
        public int Repetitions { get; set; }
        public double WeightKg { get; set; }
        public double WeightLbs { get; set; }
    }

    public class HistoryUpdateVM
    {
        public string ExerciseName { get; set; }
        public ExerciseData ExerciseData { get; set; }
    }
}
