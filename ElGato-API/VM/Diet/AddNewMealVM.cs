using System.ComponentModel.DataAnnotations;

namespace ElGato_API.VM.Diet
{
    public class AddNewMealVM
    {        
        public string? MealName { get; set; }

        [Required(ErrorMessage = "Date is required.")]
        public DateTime Date { get; set; }
    }
}
