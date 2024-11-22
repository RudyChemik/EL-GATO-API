using ElGato_API.ModelsMongo.Diet;
using System.ComponentModel.DataAnnotations;

namespace ElGato_API.VM.Diet
{
    public class SaveIngridientMealVM
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public List<Ingridient> Ingridients { get; set; }
    }
}
