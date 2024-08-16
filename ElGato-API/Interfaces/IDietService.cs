using ElGato_API.VMO.Diet;
using ElGato_API.VMO.ErrorResponse;
using ElGato_API.VMO.Questionary;

namespace ElGato_API.Interfaces
{
    public interface IDietService
    {
        Task<(IngridientVMO? ingridient, BasicErrorResponse error)> GetIngridientByEan(string ean);
        CalorieIntakeVMO CalculateCalories(QuestionaryVM questionary);
    }
}
