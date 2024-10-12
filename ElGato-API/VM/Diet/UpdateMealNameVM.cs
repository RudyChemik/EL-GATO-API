using System.ComponentModel.DataAnnotations;

namespace ElGato_API.VM.Diet
{
    public class UpdateMealNameVM
    {
        [Required(ErrorMessage = "Product name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Meal id is required")]
        public int MealPublicId { get; set; }

        [Required(ErrorMessage = "Date is required")]
        public DateTime Date { get; set; }
    }
}
