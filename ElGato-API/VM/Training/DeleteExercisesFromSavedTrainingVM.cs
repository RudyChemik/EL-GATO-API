using System.ComponentModel.DataAnnotations;

namespace ElGato_API.VM.Training
{
    public class DeleteExercisesFromSavedTrainingVM
    {
        [Required(ErrorMessage = "saved training public id is required to perform delete action.")]
        public int SavedTrainingPublicId { get; set; }

        [Required(ErrorMessage = "list of items to remove is required")]
        [MinLength(1,ErrorMessage = "at least one item to remove is required")]
        public List<int> ExercisesPublicIdToRemove { get; set; }
    }
}
