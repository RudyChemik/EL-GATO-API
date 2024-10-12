using ElGato_API.VMO.Diet;
using System.ComponentModel.DataAnnotations;

namespace ElGato_API.VM.Diet
{
    public class AddIngridientsVM
    {
        [Required(ErrorMessage = "MealId is required")]
        public int MealId { get; set; }

        [Required(ErrorMessage = "date is required")]
        public DateTime date { get; set; }

        [Required(ErrorMessage = "Ingridient model is required")]
        public List<IngridientVMO> Ingridient { get; set; }
    }
}
