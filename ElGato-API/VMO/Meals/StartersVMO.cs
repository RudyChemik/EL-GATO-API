namespace ElGato_API.VMO.Meals
{
    public class StartersVMO
    {
        public List<SimpleMealVMO> MostLiked { get; set; } = new List<SimpleMealVMO>();
        public List<SimpleMealVMO> Breakfast { get; set; } = new List<SimpleMealVMO>();
        public List<SimpleMealVMO> SideDish { get; set; } = new List<SimpleMealVMO>();
        public List<SimpleMealVMO> MainDish { get; set; } = new List<SimpleMealVMO>();
        public List<SimpleMealVMO> HighProtein { get; set; } = new List<SimpleMealVMO>();
        public List<SimpleMealVMO> LowCarbs { get; set; } = new List<SimpleMealVMO>();
        public List<SimpleMealVMO> HighCarb { get; set; } = new List<SimpleMealVMO>();
        public List<SimpleMealVMO> LowFats { get; set; } = new List<SimpleMealVMO>();
        public List<SimpleMealVMO> All { get; set; } = new List<SimpleMealVMO>();
    }
}
