using System.ComponentModel.DataAnnotations;

namespace ElGato_API.VM.Training
{
    public class UpdateSavedTrainingName
    {
        [Required(ErrorMessage = "New name is required for performing update")]
        public string NewName { get; set; }

        [Required(ErrorMessage = "PublicId is required")]
        public int PublicId { get; set; }
    }
}
