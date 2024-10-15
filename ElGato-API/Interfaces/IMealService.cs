using ElGato_API.ModelsMongo.Meal;
using ElGato_API.VMO.ErrorResponse;
using ElGato_API.VMO.Meals;

namespace ElGato_API.Interfaces
{
    public interface IMealService
    {
        Task<(List<SimpleMealVMO> res, BasicErrorResponse error)> GetByMainCategory(List<string> LikedMeals, List<string> SavedMeals, string? category, int? qty = 5);
        Task<(List<SimpleMealVMO> res, BasicErrorResponse error)> GetByLowMakro(List<string> LikedMeals, List<string> SavedMeals, string makroComponent, int? qty = 5);
        Task<(List<SimpleMealVMO> res, BasicErrorResponse error)> GetByHighMakro(List<string> LikedMeals, List<string> SavedMeals, string makroComponent, int? qty = 5);
        Task<(List<SimpleMealVMO> res, BasicErrorResponse error)> GetMostLiked(List<string> LikedMeals, List<string> SavedMeals, int? qty = 5);
        Task<(List<SimpleMealVMO> res, BasicErrorResponse error)> GetRandom(List<string> LikedMeals, List<string> SavedMeals, int? qty = 5);

        Task<(MealLikesDocument res, BasicErrorResponse error)> GetUserMealLikeDoc(string userId);
    }
}
