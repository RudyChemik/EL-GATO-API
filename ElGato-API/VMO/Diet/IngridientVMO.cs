namespace ElGato_API.VMO.Diet
{
    public class IngridientVMO
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public double Prep_For { get; set; }
        public double WeightValue { get; set; }
        public bool Servings { get; set; } = false;
        public double Proteins { get; set; }
        public double Carbs { get; set; }
        public double Fats { get; set; }
        public string? Brand { get; set; }

        public double Kcal { get; set; }
        public double EnergyKj
        {
            get
            {
                return Math.Round(Kcal * 4.18,2);
            }
        }
    }
}
