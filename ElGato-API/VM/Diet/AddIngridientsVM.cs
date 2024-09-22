using ElGato_API.VMO.Diet;

namespace ElGato_API.VM.Diet
{
    public class AddIngridientsVM
    {
        public int MealId { get; set; }
        public DateTime date { get; set; }
        public List<IngridientVMO> Ingridient { get; set; }
    }
}
