using System.ComponentModel.DataAnnotations;

namespace ElGato_API.VM.Training
{
    public class SaveTrainingVM
    {
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }
        public List<string> ExerciseNames { get; set; } = new List<string>();
    }
}
