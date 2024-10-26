namespace ElGato_API.VM.Meal
{
    public class SearchMealVM
    {
        public int? Qty {get; set;} = 50;
        public int? PageNumber { get; set; } = 1;
        public string Phrase { get; set; }
        public SearchNutritions? Nutritions {  get; set; } 
        public SearchTimeRange? SearchTimeRange { get; set; }
        public int? SortValue { get; set; } //A-Z 1 //Z-A 2 //Most likes - 3 // Kcal inc - 4 //kcal dec -5
    }

    public class SearchNutritions 
    {
        public double MinimalCalories { get; set; } = 0;
        public double MaximalCalories { get; set; } = 9999;
        public double MinimalProtein { get; set; } = 0;
        public double MaximalProtein { get;set; } = 9999;
        public double MinimumFats { get; set; } = 0;
        public double MaximumFats { get; set; } = 9999;
        public double MinimumCarbs { get; set; } = 0;
        public double MaximumCarbs { get; set; } = 9999;
    }

    public class SearchTimeRange 
    { 
        public double MinimalTime { get; set; } = 0;
        public double MaximumTime { get; set; } = 100000;
    }

}
