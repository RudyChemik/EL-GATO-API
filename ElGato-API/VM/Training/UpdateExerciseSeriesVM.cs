using ElGato_API.ModelsMongo.Training;
using System.ComponentModel.DataAnnotations;

namespace ElGato_API.VM.Training
{
    public class UpdateExerciseSeriesVM
    {
        [Required(ErrorMessage = "Exercise id is required for performing patch action.")]
        public int ExercisePublicId { get; set; }

        [Required(ErrorMessage = "Date is required")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Series model is required.")]
        [MinLength(1, ErrorMessage = "At least one serie data is required to perform patch.")]
        public List<UpdateSeries> SeriesToUpdate {  get; set; }

        [Required(ErrorMessage = "Whole exercise data from current training day is required")]
        public HistoryUpdateVM HistoryUpdate { get; set; }
    }

    public class UpdateSeries
    {
        public int SerieId { get; set; }
        public int NewRepetitions { get; set; }
        public double NewWeightKg { get; set; }
        public double NewWeightLbs { get; set; }
        public ExerciseSerieTempo? newTempo { get; set; }
    }
}
