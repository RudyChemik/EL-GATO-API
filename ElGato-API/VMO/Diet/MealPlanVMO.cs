using ElGato_API.ModelsMongo.Diet;

namespace ElGato_API.VMO.Diet
{
    public class MealPlanVMO
    {
        public string Name { get; set; }
        public bool IsSaved { get; set; } = false;
        public int PublicId { get; set; }
        public List<Ingridient> Ingridient { get; set; } = new List<Ingridient>();
    }
}
