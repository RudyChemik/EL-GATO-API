using ElGato_API.ModelsMongo.Diet;
using ElGato_API.VM.Diet;
using ElGato_API.VMO.Diet;
using ElGato_API.VMO.ErrorResponse;
using ElGato_API.VMO.Questionary;

namespace ElGato_API.Interfaces
{
    public interface IDietService
    {
        //POST
        Task<BasicErrorResponse> AddNewMeal(string userId, string mealName, DateTime date);
        Task<BasicErrorResponse> AddIngridientToMeal(string userId, AddIngridientVM model);
        Task<BasicErrorResponse> AddIngredientsToMeals(string userId, AddIngridientsVM model);
        Task<BasicErrorResponse> AddWater(string userId, int water, DateTime date);
        Task<BasicErrorResponse> AddMealToSavedMeals(string userId, SaveIngridientMealVM model);
        Task<BasicErrorResponse> AddMealFromSavedMeals(string userId, AddMealFromSavedVM model);

        //getters
        Task<(IngridientVMO? ingridient, BasicErrorResponse error)> GetIngridientByEan(string ean);
        Task<(List<IngridientVMO>? ingridients, BasicErrorResponse error)> GetListOfIngridientsByName(string name);
        Task<(BasicErrorResponse errorResponse, DietDocVMO model)> GetUserDoc(string userId);
        Task<(BasicErrorResponse errorResponse, DietDayVMO model)> GetUserDietDay(string userId, DateTime date);
        Task<(BasicErrorResponse errorResponse, List<MealPlan>? model)> GetSavedMeals(string userId);

        //del
        Task<BasicErrorResponse> DeleteMeal(string userId, int publicId, DateTime date);
        Task<BasicErrorResponse> DeleteIngridientFromMeal(string userId, RemoveIngridientVM model);
        Task<BasicErrorResponse> RemoveMealFromSaved(string userId, string name);

        //patch
        Task<BasicErrorResponse> UpdateMealName(string userId, UpdateMealNameVM model);
        Task<BasicErrorResponse> UpdateIngridientWeightValue(string userId, UpdateIngridientVM model);

        CalorieIntakeVMO CalculateCalories(QuestionaryVM questionary);
    }
}
