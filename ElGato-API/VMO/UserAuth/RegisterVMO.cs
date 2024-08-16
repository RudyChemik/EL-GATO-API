using ElGato_API.VMO.Diet;
using Microsoft.AspNetCore.Identity;

namespace ElGato_API.VMO.UserAuth
{
    public class RegisterVMO
    {
        public CalorieIntakeVMO? calorieIntake { get; set; }
        public string JWT { get; set; }
        public bool Success { get; set; }
        public IEnumerable<IdentityError> Errors { get; set; }
    }
}
