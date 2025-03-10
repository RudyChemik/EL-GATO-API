namespace ElGato_API.VMO.User
{
    public class DailyMakroDistributionVMO
    {
        public DateTime Date { get; set; }
        public List<DailyDistributionMeals> Meals { get; set; } = new List<DailyDistributionMeals>();
    }

    public class DailyDistributionMeals
    {
        public string Name { get; set; }
        public DailyDistribution Distribution { get; set; } = new DailyDistribution();
        public List<DailyDistributionIngridient> Ingridients { get; set; } = new List<DailyDistributionIngridient>();
    }

    public class DailyDistributionIngridient
    {
        public string Name { get; set; }
        public double Grams { get; set; }
        public DailyDistribution Distribution { get; set; } = new DailyDistribution();
    }

    //This should be given by *ingridient**obj* weight alrd calculated - not by 100g.
    public class DailyDistribution
    {
        public double Kcal { get; set; }
        public double Protein { get; set; }
        public double Fats { get; set; }
        public double Carbs { get; set; }
    }
}
