using ElGato_API.Data;
using ElGato_API.Interfaces;
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

        public async Task<(BasicErrorResponse error, List<ChallengeVMO>? data)> GetActiveChallenges()
        {
            try
            {
                List<ChallengeVMO> vmo = new List<ChallengeVMO>();
                var challs = await _context.Challanges.Where(a=>a.EndDate > DateTime.Today).ToListAsync();

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
    }
}
