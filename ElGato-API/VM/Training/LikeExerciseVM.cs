using ElGato_API.ModelsMongo.History;

namespace ElGato_API.VM.Training
{
    public class LikeExerciseVM
    {
        public bool Own {  get; set; } = false;
        public string Name { get; set; }
        public MuscleType MuscleType { get; set; } = MuscleType.Unknown;
        public int? Id { get; set; }
    }
}
