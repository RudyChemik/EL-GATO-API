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

        //getters
        Task<(IngridientVMO? ingridient, BasicErrorResponse error)> GetIngridientByEan(string ean);
        Task<(List<IngridientVMO>? ingridients, BasicErrorResponse error)> GetListOfIngridientsByName(string name);

        //del
        Task<BasicErrorResponse> DeleteMeal(string userId, int publicId, DateTime date);

        CalorieIntakeVMO CalculateCalories(QuestionaryVM questionary);
    }
}
