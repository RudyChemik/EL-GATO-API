using System.ComponentModel.DataAnnotations;

namespace ElGato_API.VM.Training
{
    public class RemoveSeriesFromExerciseVM
    {
        [Required(ErrorMessage = "Date is required")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Public id for an exercise you want to add series to is required.")]
        public int ExercisePublicId { get; set; }

        [Required(ErrorMessage = "Whole exercise data from current training day is required")]
        public HistoryUpdateVM HistoryUpdate { get; set; }

        [Required(ErrorMessage = "List of series id is required to perform delete action")]
        [MinLength(1, ErrorMessage = "At least one series id is required")]
        public List<int> seriesIdToRemove { get; set; }
    }
}
