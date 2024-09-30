using System.ComponentModel.DataAnnotations;

namespace ElGato_API.VM.Requests
{
    public class IngredientReportRequestVM
    {
        [Required(ErrorMessage = "Reported ingredient id is required")]
        public string IngredientId { get; set; }

        [Required(ErrorMessage = "Reported ingredient IngredientName is required")]
        public string IngredientName { get; set; }
        [Required(ErrorMessage = "Reported cause is required")]
        public int Cause { get; set; }
    }
}
