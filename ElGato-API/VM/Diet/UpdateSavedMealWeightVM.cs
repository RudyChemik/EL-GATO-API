using System.ComponentModel.DataAnnotations;

namespace ElGato_API.VM.Diet
{
    public class UpdateSavedMealWeightVM
    {
        [Required(ErrorMessage = "Updating saved meal name required")]
        public string MealName { get; set; }

        [Required(ErrorMessage = "Updating ingridient name is required")]
        public string IngridientName { get; set; }

        [Required(ErrorMessage ="Updating ingridient publicId is crucial")]
        public string PublicId { get; set; }

        [Required(ErrorMessage = "New ingridient weight is required")]
        public double NewWeight { get; set; }
    }
}
