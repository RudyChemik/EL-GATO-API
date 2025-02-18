using System.ComponentModel.DataAnnotations;

namespace ElGato_API.VM.Training
{
    public class RemoveSavedTrainingsVM
    {
        [Required(ErrorMessage = "List of items to remove is required")]
        [MinLength(1, ErrorMessage = "At least one item to remove is required")]
        public List<int> SavedTrainingIdsToRemove { get; set; }
    }
}
