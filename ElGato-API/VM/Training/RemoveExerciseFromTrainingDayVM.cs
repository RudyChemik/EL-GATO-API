using System.ComponentModel.DataAnnotations;

namespace ElGato_API.VM.Training
{
    public class RemoveExerciseFromTrainingDayVM
    {
        [Required(ErrorMessage = "Date is necessary")]
        public DateTime Date { get; set; }
        [Required(ErrorMessage = "Id of an exercise to remove is necessary")]
        public int ExerciseId { get; set; }
    }
}
