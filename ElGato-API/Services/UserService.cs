using ElGato_API.Data;
using ElGato_API.Interfaces;
using ElGato_API.VMO.ErrorResponse;
using ElGato_API.VMO.User;
using Microsoft.EntityFrameworkCore;

namespace ElGato_API.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _dbContext;
        public UserService(AppDbContext dbContext) 
        { 
            _dbContext = dbContext;
        }

        public async Task<(BasicErrorResponse error, UserCalorieIntake model)> GetUserCalories(string userId)
        {
            BasicErrorResponse error = new BasicErrorResponse() { Success = false };
            UserCalorieIntake userCalorieIntake = new UserCalorieIntake();

            try
            {
                var res = await _dbContext.Users.Include(x=>x.CalorieInformation).FirstOrDefaultAsync(a=>a.Id == userId);
                if (res == null || res.CalorieInformation == null) 
                {
                    error.ErrorMessage = "User calorie intake information not found.";
                    error.ErrorCode = ErrorCodes.NotFound;
                    return (error, userCalorieIntake);
                }

                userCalorieIntake.Kcal = res.CalorieInformation.Kcal;
                userCalorieIntake.Carbs = res.CalorieInformation.Carbs;
                userCalorieIntake.Fats = res.CalorieInformation.Fat;
                userCalorieIntake.Protein = res.CalorieInformation.Protein;

                error.Success = true;
                error.ErrorCode = ErrorCodes.None;
                return (error, userCalorieIntake);
            }
            catch (Exception ex) 
            { 
                error.ErrorMessage = ex.Message;
                error.ErrorCode = ErrorCodes.Internal;
                return (error, userCalorieIntake);
            }
        }
    }
}
