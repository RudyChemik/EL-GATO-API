using System.ComponentModel.DataAnnotations;

namespace ElGato_API.VM.Diet
{
    public class UpdateIngridientVM
    {
        [Required(ErrorMessage = "Product name is required")]
        public string IngridientName { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        public string IngridientId { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        public double IngridientWeightOld { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Id value must be positive")]
        public double IngridientWeightNew { get; set; }

        [Required(ErrorMessage = "Date is required")]
        public DateTime Date {  get; set; }

        [Required(ErrorMessage = "id is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Id value must be positive")]
        public int MealPublicId { get; set; }
    }
}
