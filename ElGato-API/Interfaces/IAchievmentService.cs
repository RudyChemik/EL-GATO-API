using ElGato_API.VMO.Achievments;
using ElGato_API.VMO.ErrorResponse;

namespace ElGato_API.Interfaces
{
    public interface IAchievmentService
    {
        Task<(BasicErrorResponse error, string? achievmentName)> GetCurrentAchivmentIdFromFamily(string achievmentFamily, string userId);
        Task<(BasicErrorResponse error, AchievmentResponse? ach)> IncrementAchievmentProgress(string achievmentName, string userId);
    }
}
