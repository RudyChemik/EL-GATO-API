using ElGato_API.ModelsMongo.History;
using System.ComponentModel.DataAnnotations;

namespace ElGato_API.VM.Training
{
    public class NewPersonalExerciseVM
    {
        [Required(ErrorMessage = "New personal exercise name is required.")]
        public string ExerciseName { get; set; }
        public MuscleType MuscleType { get; set; } = MuscleType.Unknown;
    }
}
