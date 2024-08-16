using ElGato_API.VM;
using ElGato_API.VM.User_Auth;
using ElGato_API.VMO.Diet;
using ElGato_API.VMO.UserAuth;
using Microsoft.AspNetCore.Identity;

namespace ElGato_API.Interfaces
{
    public interface IAccountService
    {
        Task<LoginVMO> LoginUser(LoginVM loginVM);
        Task<IdentityResult> RegisterUser(RegisterWithQuestVM model, CalorieIntakeVMO calorieIntake);
        Task<bool> IsEmailAlreadyUsed(string email);
    }
}
