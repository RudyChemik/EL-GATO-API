using System.ComponentModel.DataAnnotations;

namespace ElGato_API.Models.Training
{
    public class Muscle
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string NormalName { get; set; }
        public MuscleGroup Group {  get; set; } 
    }

    public enum MuscleGroup
    {
        Chest,          
        Shoulders,      
        Biceps,
        Forearms,
        Triceps,
        Back,           
        Core,           
        Legs,           
        Calves,         
        Hips
    }
}
