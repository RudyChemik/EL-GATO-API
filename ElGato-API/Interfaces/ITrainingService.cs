using ElGato_API.Models.Training;
using ElGato_API.VM.Training;
using ElGato_API.VMO.ErrorResponse;
using ElGato_API.VMO.Training;

namespace ElGato_API.Interfaces
{
    public interface ITrainingService
    {
        Task<(BasicErrorResponse error, List<ExerciseVMO>? data)> GetAllExercises();
        Task<(BasicErrorResponse error, List<LikedExercisesVMO>? data)> GetAllLikedExercises(string userId);
        Task<(BasicErrorResponse error, TrainingDayVMO? data)> GetUserTrainingDay(string userId, DateTime date);
        Task<BasicErrorResponse> LikeExercise(string userId, LikeExerciseVM model);
        Task<BasicErrorResponse> RemoveExercisesFromLiked(string userId, List<LikeExerciseVM> model);
    }
}
