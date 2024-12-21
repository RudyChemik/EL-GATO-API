using ElGato_API.Models.Training;

namespace ElGato_API.VMO.Training
{
    public class ExerciseVMO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }
        public string Image { get; set; }
        public string? ImgGifPart { get; set; }
        public List<MuscleVMO> MusclesEngaged { get; set; } = new List<MuscleVMO>();
        public string MainBodyPart { get; set; }
        public string SpecificBodyPart { get; set; }
        public string Equipment { get; set; }
        public string Difficulty { get; set; }
    }
}
