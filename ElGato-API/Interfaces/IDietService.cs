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
        Task<BasicErrorResponse> AddWater(string userId, int water, DateTime date);

        //getters
        Task<(IngridientVMO? ingridient, BasicErrorResponse error)> GetIngridientByEan(string ean);
        Task<(List<IngridientVMO>? ingridients, BasicErrorResponse error)> GetListOfIngridientsByName(string name);
        Task<(BasicErrorResponse errorResponse, DietDocVMO model)> GetUserDoc(string userId);
        Task<(BasicErrorResponse errorResponse, DietDayVMO model)> GetUserDietDay(string userId, DateTime date);

        //del
        Task<BasicErrorResponse> DeleteMeal(string userId, int publicId, DateTime date);
        Task<BasicErrorResponse> DeleteIngridientFromMeal(string userId, RemoveIngridientVM model);

        //patch
        Task<BasicErrorResponse> UpdateMealName(string userId, UpdateMealNameVM model);
        Task<BasicErrorResponse> UpdateIngridientWeightValue(string userId, UpdateIngridientVM model);

        CalorieIntakeVMO CalculateCalories(QuestionaryVM questionary);
    }
}
