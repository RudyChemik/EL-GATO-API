using System.ComponentModel.DataAnnotations;

namespace ElGato_API.VM.UserData
{
    public class GetPasstExerciseDataVM
    {
        [Required(ErrorMessage = "List of exercises names is required.")]
        [MinLength(1, ErrorMessage = "List cannot be empty.")]
        public List<string> ExercisesNames { get; set; }
    }
}
