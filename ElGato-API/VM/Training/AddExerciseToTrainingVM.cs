using System.ComponentModel.DataAnnotations;

namespace ElGato_API.VM.Training
{
    public class AddExerciseToTrainingVM
    {
        [Required(ErrorMessage = "Date is required")]
        public DateTime Date {  get; set; }

        [Required(ErrorMessage = "Exercise list is required")]
        public List<string> Name { get; set; }
    }
}
