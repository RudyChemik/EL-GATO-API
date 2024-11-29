using System.ComponentModel.DataAnnotations;

namespace ElGato_API.VM.Diet
{
    public class DeleteMealVM
    {
        [Required(ErrorMessage = "Public id is required")]
        public int PublicId { get; set; }

        [Required(ErrorMessage = "Date is required")]
        public DateTime Date { get; set; }
    }
}
