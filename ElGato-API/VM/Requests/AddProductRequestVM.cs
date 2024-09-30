using System.ComponentModel.DataAnnotations;

namespace ElGato_API.VM.Requests
{
    public class AddProductRequestVM
    {
        [Required(ErrorMessage = "Product name is required")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Product brand is required")]
        public string ProductBrand { get; set; }

        [Required]
        [RegularExpression(@"\d{13}", ErrorMessage = "EAN-13 value must be correct")]
        public string ProductEan13 { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Proteins value must be positive")]
        public double Proteins { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Carbs value must be positive")]
        public double Carbs { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Fats value must be positive")]
        public double Fats { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Kcal value must be positive")]
        public double EnergyKcal { get; set; }
    }
}
