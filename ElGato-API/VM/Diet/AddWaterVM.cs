using System.ComponentModel.DataAnnotations;

namespace ElGato_API.VM.Diet
{
    public class AddWaterVM
    {
        [Required(ErrorMessage = "Water amount is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Water amount must be greater than 0")]
        public int Water {  get; set; }
        [Required(ErrorMessage = "Date is required")]
        public DateTime Date { get; set; }
    }
}
