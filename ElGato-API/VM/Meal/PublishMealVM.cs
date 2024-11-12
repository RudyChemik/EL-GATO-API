using Amazon.Util.Internal;
using ElGato_API.ModelsMongo.Meal;
using System.ComponentModel.DataAnnotations;

namespace ElGato_API.VM.Meal
{
    public class PublishMealVM
    {
        [Required(ErrorMessage = "Recipe name is required")]
        public string Name { get; set; }
        public string Desc { get; set; }
        public string? Time { get; set; }
        public IFormFile? Image { get; set; }

        [Required(ErrorMessage = "Ingridient list is required")]
        public List<string> Ingridients { get; set; }

        [Required(ErrorMessage = "Steps are required")]
        public List<string> Steps { get; set; }
        public List<string>? Tags { get; set; }

        [Required(ErrorMessage = "Makro is required")]
        public MealsMakro Makro { get; set; }
    }
}
