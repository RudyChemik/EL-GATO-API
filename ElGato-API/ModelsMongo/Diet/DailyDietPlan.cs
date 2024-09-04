using ElGato_API.Models.User;

namespace ElGato_API.ModelsMongo.Diet
{
    public class DailyDietPlan
    {
        public DateTime Date { get; set; }
        public CalorieInformation Makros { get; set; }
        public int Water { get; set; }
        public List<MealPlan> Meals { get; set; } = new List<MealPlan>();
    }
}
