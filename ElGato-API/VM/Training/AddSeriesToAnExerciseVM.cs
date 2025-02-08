using System.ComponentModel.DataAnnotations;

namespace ElGato_API.VM.Training
{
    public class AddSeriesToAnExerciseVM
    {
        [Required(ErrorMessage = "Date is required")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Public id for an exercise you want to add series to is required.")]
        public int PublicId { get; set; }

        public List<AddSeriesVM> Series { get; set; } = new List<AddSeriesVM>();
    }

    public class AddSeriesVM
    {
        public int Repetitions { get; set; }
        public double WeightKg { get; set; }
        public double WeightLbs { get; set; }
    }
}
