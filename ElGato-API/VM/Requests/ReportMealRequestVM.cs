using System.ComponentModel.DataAnnotations;

namespace ElGato_API.VM.Requests
{
    public class ReportMealRequestVM
    {
        [Required(ErrorMessage = "Reported meal id is required")]
        public string MealId { get; set; }

        [Required(ErrorMessage = "Reported meal name is required")]
        public string MealName { get; set; }

        [Required(ErrorMessage = "Reported cause is required")]
        public int Cause { get; set; }
    }
}
