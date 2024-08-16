namespace ElGato_API.Models.User
{
    public class CalorieInformation
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public AppUser AppUser { get; set; }
        public double Fat { get; set; }
        public double Carbs { get; set; }
        public double Protein { get; set; }
        public double Kcal { get; set; } = 0;
        public double Kj { get; set; } = 0;
    }
}
