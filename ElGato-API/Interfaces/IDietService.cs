using ElGato_API.VMO.Diet;
using ElGato_API.VMO.ErrorResponse;
using ElGato_API.VMO.Questionary;

namespace ElGato_API.Interfaces
{
    public interface IDietService
    {
        //POST
        Task<BasicErrorResponse> AddNewMeal(string userId, string mealName, DateTime date);

        //getters
        Task<(IngridientVMO? ingridient, BasicErrorResponse error)> GetIngridientByEan(string ean);

        //del
        Task<BasicErrorResponse> DeleteMeal(string userId, int publicId, DateTime date);

        CalorieIntakeVMO CalculateCalories(QuestionaryVM questionary);
    }
}
