using System.ComponentModel.DataAnnotations;

namespace ElGato_API.VM.Training
{
    public class AddSavedTrainingToTrainingDayVM
    {
        [Required(ErrorMessage = "id of saved training is required")]
        public int SavedTrainingId {  get; set; }

        [Required(ErrorMessage = "Date is required")]
        public DateTime Date { get; set; }
    }
}
