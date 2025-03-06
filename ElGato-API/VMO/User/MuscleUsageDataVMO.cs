using ElGato_API.ModelsMongo.History;

namespace ElGato_API.VMO.User
{
    public class MuscleUsageDataVMO
    {
        public List<MuscleUsage> muscleUsage {  get; set; } = new List<MuscleUsage>();
    }

    public class MuscleUsage
    {
        public MuscleType MuscleType { get; set; }
        public List<DateTime> Dates { get; set; }
    }
}
