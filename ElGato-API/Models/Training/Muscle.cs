using System.ComponentModel.DataAnnotations;

namespace ElGato_API.Models.Training
{
    public class Muscle
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
