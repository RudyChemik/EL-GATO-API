using ElGato_API.ModelsMongo.Diet;
using ElGato_API.VMO.ErrorResponse;

namespace ElGato_API.VMO.Diet
{
    public class DietDocVMO
    {
        public List<DailyDietPlan> DailyPlans { get; set; } = new List<DailyDietPlan>();
    }
}
