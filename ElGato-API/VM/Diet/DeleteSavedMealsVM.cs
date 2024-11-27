using System.ComponentModel.DataAnnotations;

namespace ElGato_API.VM.Diet
{
    public class DeleteSavedMealsVM
    {
        [Required(ErrorMessage = "Meals name are required to perform delete action")]
        public List<string> SavedMealsNames { get; set; }
    }
}
