using System.ComponentModel.DataAnnotations;

namespace ElGato_API.Models.Training
{
    public class Exercises
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }
        public string Image { get; set; }
        public string? ImageGifPart { get; set; }
        public List<Muscle> MusclesEngaded { get; set; }
        public MainBodyPart MainBodyPart { get; set; }
        public SpecificBodyPart SpecificBodyPart { get; set; }
        public Equipment Equipment { get; set; }
        public DifficultyLevel Difficulty { get; set; }
    }

    public enum DifficultyLevel
    {
        Easy,
        Medium,
        Hard
    }

    public enum MainBodyPart
    {
        UpperBody,
        LowerBody,
        Core,
        FullBody,
        Other
    }

    public enum SpecificBodyPart
    {
        Chest,
        UpperChest,
        LowerChest,
        Back,
        Lats,
        Traps,
        Biceps,
        Triceps,
        Shoulders,
        Quads,
        Hamstrings,
        Glutes,
        Calves,
        Abs,
        Obliques,
        Forearms,
    }

    public enum Equipment
    {
        None,
        Body,
        Cables,
        Dumbbells,
        Machine,
        Barbel,
        Other,
    }
}
