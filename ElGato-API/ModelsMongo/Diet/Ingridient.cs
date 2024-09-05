namespace ElGato_API.ModelsMongo.Diet
{
    public class Ingridient
    {
        public string Name { get; set; }
        public int publicId { get; set; }
        public double WeightValue { get; set; }
        public double Proteins { get; set; }
        public double Carbs { get; set; }
        public double Fats { get; set; }
        public double EnergyKcal { get; set; }
        public double EnergyKj
        {
            get
            {
                return Math.Round(EnergyKcal * 4.18, 2);
            }
        }
    }
}
