namespace ElGato_API.VMO.User
{
    public class MakroDataVMO
    {
        public List<MakroData> MakroData { get; set; } = new List<MakroData>();
    }

    public class MakroData
    {
        public DateTime Date { get; set; }
        public double EnergyKj { get; set; }
        public double EnergyKcal { get; set; }
        public double Proteins { get; set; }
        public double Carbs { get; set; }
        public double Fats { get; set; }
    }
}
