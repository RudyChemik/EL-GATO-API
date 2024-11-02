using System.ComponentModel.DataAnnotations;

namespace ElGato_API.VM.Meal
{
    public class ExtendedStartersVM
    {
        [Required(ErrorMessage = "Starter type is required")]
        public string Type { get; set; }
        public int? page { get; set; } = 1;
        public int? pageSize { get; set; } = 50;
    }
}
