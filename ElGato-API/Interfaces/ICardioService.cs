using ElGato_API.VMO.Cardio;
using ElGato_API.VMO.ErrorResponse;

namespace ElGato_API.Interfaces
{
    public interface ICardioService
    {
        Task<(BasicErrorResponse error, List<ChallengeVMO>? data)> GetActiveChallenges(string userId);
        Task<(BasicErrorResponse error, List<ActiveChallengeVMO>? data)> GetUserActiveChallenges(string userId);
        Task<BasicErrorResponse> JoinChallenge(string userId, int challengeId);
    }
}
