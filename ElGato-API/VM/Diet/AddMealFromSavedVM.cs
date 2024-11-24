using System.ComponentModel.DataAnnotations;

namespace ElGato_API.VM.Diet
{
    public class AddMealFromSavedVM
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public DateTime Date { get; set; }
    }
}
