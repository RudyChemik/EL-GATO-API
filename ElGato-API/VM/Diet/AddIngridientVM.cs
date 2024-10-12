using ElGato_API.VMO.Diet;
using System.ComponentModel.DataAnnotations;

namespace ElGato_API.VM.Diet
{
    public class AddIngridientVM
    {
        [Required(ErrorMessage = "Meal id is required")]
        public int MealId { get; set; }

        [Required(ErrorMessage = "date is required")]
        public DateTime date { get; set; }

        [Required(ErrorMessage = "WeightValue is required")]
        [Range(0, double.MaxValue, ErrorMessage = "WeightValue must be positive")]
        public double WeightValue { get; set; }

        [Required(ErrorMessage = "Ingridient model is required")]
        public IngridientVMO Ingridient { get; set; }
    }
}
