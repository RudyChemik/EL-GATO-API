using ElGato_API.VMO.ErrorResponse;
using ElGato_API.VMO.User;

namespace ElGato_API.Interfaces
{
    public interface IUserService
    {
        Task<(BasicErrorResponse error, UserCalorieIntake model)> GetUserCalories(string userId);
    }
}
