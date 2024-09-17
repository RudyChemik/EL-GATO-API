using ElGato_API.ModelsMongo.Diet;

namespace ElGato_API.VMO.Diet
{
    public class DietDayVMO
    {
        public DateTime Date { get; set; }
        public int Water { get; set; }
        public List<MealPlan> Meals { get; set; } = new List<MealPlan>();
        public DailyCalorieCount CalorieCounter { get; set; }
    }

    public class DailyCalorieCount 
    { 
        public double Kcal { get; set; }
        public double Protein { get; set; }
        public double Fats { get; set; }
        public double Carbs { get; set; }
    }
}
