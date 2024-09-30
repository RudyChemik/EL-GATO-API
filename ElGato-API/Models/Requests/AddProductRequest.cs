using System.ComponentModel.DataAnnotations;

namespace ElGato_API.Models.Requests
{
    public class AddProductRequest
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }
        public string ProductName { get; set; }
        public string ProductBrand { get; set; }
        public string ProductEan13 { get; set; }
        public double PrepedFor { get; set; } = 100;
        public double Proteins { get; set; }
        public double Carbs { get; set; }
        public double Fats { get; set; }
        public double EnergyKcal { get; set; }
    }
}
