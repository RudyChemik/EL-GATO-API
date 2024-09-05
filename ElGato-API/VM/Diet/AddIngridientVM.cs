using ElGato_API.VMO.Diet;

namespace ElGato_API.VM.Diet
{
    public class AddIngridientVM
    {
        public int MealId { get; set; }
        public DateTime date { get; set; }
        public double WeightValue { get; set; }
        public IngridientVMO Ingridient { get; set; }
    }
}
