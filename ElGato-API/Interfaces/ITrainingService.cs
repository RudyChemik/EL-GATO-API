using ElGato_API.Models.Training;
using ElGato_API.VMO.ErrorResponse;
using ElGato_API.VMO.Training;

namespace ElGato_API.Interfaces
{
    public interface ITrainingService
    {
        Task<(BasicErrorResponse error, List<ExerciseVMO>? data)> GetAllExercises();
    }
}
