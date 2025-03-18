using ElGato_API.Data;
using ElGato_API.Interfaces;
using ElGato_API.Models.User;
using ElGato_API.VMO.Cardio;
using ElGato_API.VMO.ErrorResponse;
using Microsoft.EntityFrameworkCore;

namespace ElGato_API.Services
{
    public class CardioService : ICardioService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CardioService> _logger;
        public CardioService(AppDbContext context, ILogger<CardioService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(BasicErrorResponse error, List<ChallengeVMO>? data)> GetActiveChallenges(string userId)
        {
            try
            {
                List<ChallengeVMO> vmo = new List<ChallengeVMO>();
                var challs = await _context.Challanges.Where(a=>a.EndDate > DateTime.Today).ToListAsync();
                var participatedChallenges = await _context.AppUser.Include(c=>c.ActiveChallanges).Where(a=>a.Id == userId).FirstOrDefaultAsync();

                if (participatedChallenges != null && participatedChallenges.ActiveChallanges != null)
                {
                    challs.RemoveAll(c => participatedChallenges.ActiveChallanges.Any(ac => ac.ChallengeId == c.Id));
                }

                foreach (var chall in challs)
                {
                    var newRec = new ChallengeVMO()
                    {
                        Id = chall.Id,
                        Name = chall.Name,
                        Badge = chall.Badge,
                        Description = chall.Description,
                        EndDate = chall.EndDate,
                        GoalType = chall.GoalType,
                        GoalValue = chall.GoalValue,
                        MaxTimeMinutes = chall.MaxTimeMinutes,
                        Type = chall.Type
                    };

                    if (chall.Creator != null)
                    {
                        newRec.Creator = new CreatorVMO()
                        {
                            Description = chall.Creator.Description,
                            Name = chall.Creator.Name,
                            Pfp = chall.Creator.Pfp,
                        };
                    }

                    vmo.Add(newRec);
                }

                return (new BasicErrorResponse() { ErrorCode = ErrorCodes.None, ErrorMessage = "Sucess", Success = true}, vmo);
            } 
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed while trying to get active challenges. Method: {nameof(GetActiveChallenges)}");
                return (new BasicErrorResponse() { ErrorCode = ErrorCodes.Internal, ErrorMessage = $"Error occured: {ex.Message}", Success = false }, null);
            }
        }

        public async Task<(BasicErrorResponse error, List<ActiveChallengeVMO>? data)> GetUserActiveChallenges(string userId)
        {
            try
            {
                var vmo = new List<ActiveChallengeVMO>();
                var user = await _context.AppUser.Include(a=>a.ActiveChallanges).ThenInclude(ac => ac.Challenge).FirstOrDefaultAsync(a=>a.Id == userId);
                if (user != null && user.ActiveChallanges != null)
                {
                    foreach (var activeChallenge in user.ActiveChallanges)
                    {
                        if(activeChallenge.Challenge.EndDate < DateTime.UtcNow)
                        {
                            continue;
                        }

                        var challengeVMO = new ChallengeVMO
                        {
                            Id = activeChallenge.Challenge.Id,
                            Name = activeChallenge.Challenge.Name,
                            Description = activeChallenge.Challenge.Description,
                            EndDate = activeChallenge.Challenge.EndDate,
                            Badge = activeChallenge.Challenge.Badge,    
                            GoalType = activeChallenge.Challenge.GoalType,
                            GoalValue = activeChallenge.Challenge.GoalValue,
                            MaxTimeMinutes = activeChallenge.Challenge.MaxTimeMinutes,
                            Type = activeChallenge.Challenge.Type
                        };

                        if(activeChallenge.Challenge.Creator != null) 
                        {
                            var creator = new CreatorVMO()
                            {
                                Description = activeChallenge.Challenge.Creator.Description,
                                Name = activeChallenge.Challenge.Creator.Name,
                                Pfp = activeChallenge.Challenge.Creator.Pfp,
                            };

                            challengeVMO.Creator = creator;
                        }

                        vmo.Add(new ActiveChallengeVMO
                        {
                            ChallengeData = challengeVMO,
                            CurrentProgess = activeChallenge.CurrentProgress,
                            StartDate = activeChallenge.StartDate
                        });
                    }
                }

                return (new BasicErrorResponse() { ErrorCode = ErrorCodes.None, Success = true, ErrorMessage = "Sucess"}, vmo);
            } 
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed while trying to get currently active challenges for user. UserId: {userId} Method: {nameof(GetActiveChallenges)}");
                return (new BasicErrorResponse() { ErrorCode = ErrorCodes.Internal, ErrorMessage = $"Error occured: {ex.Message}", Success = false }, null);
            }
        }

        public async Task<BasicErrorResponse> JoinChallenge(string userId, int challengeId)
        {
            try
            {
                var challenge = await _context.Challanges.FirstOrDefaultAsync(a=>a.Id == challengeId);
                if(challenge == null)
                {
                    _logger.LogWarning($"User tried to join un-existing challenge. UserId: {userId} ChallengeId: {challengeId} Method: {nameof(JoinChallenge)}");
                    return new BasicErrorResponse() { ErrorCode = ErrorCodes.NotFound, ErrorMessage = $"Challenge with id {challengeId} doesn ot exists.", Success = false };
                }

                var user = await _context.AppUser.FirstOrDefaultAsync(a=>a.Id == userId);

                var newActiveChallengeRecord = new ActiveChallange()
                {
                    StartDate = DateTime.Now,
                    Challenge = challenge,
                    ChallengeId = challengeId,
                    CurrentProgress = 0
                };

                user.ActiveChallanges.Add(newActiveChallengeRecord);
                await _context.SaveChangesAsync();

                return new BasicErrorResponse() { ErrorCode = ErrorCodes.None, Success = true, ErrorMessage = "Sucess" };
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Failed while trying to join challenge. UserId: {userId} ChallengeId: {challengeId} Method: {nameof(JoinChallenge)}");
                return new BasicErrorResponse() { ErrorCode = ErrorCodes.Internal, ErrorMessage = $"Error occured: {ex.Message}", Success = false };
            }
        }
    }
}
