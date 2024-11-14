using ElGato_API.Data;
using ElGato_API.Interfaces;
using ElGato_API.Models.User;
using ElGato_API.VMO.Achievments;
using ElGato_API.VMO.ErrorResponse;
using Microsoft.EntityFrameworkCore;

namespace ElGato_API.Services
{
    public class AchievmentService : IAchievmentService
    {
        private readonly AppDbContext _context;

        public AchievmentService(AppDbContext context) 
        { 
            _context = context;
        }

        public async Task<(BasicErrorResponse error, string? achievmentName)> GetCurrentAchivmentIdFromFamily(string achievmentFamily, string userId)
        {
            try
            {
                var user = await _context.AppUser.Include(a => a.Achievments).Where(a => a.Id == userId).FirstOrDefaultAsync();

                if (user == null)
                {
                    return (new BasicErrorResponse { Success = false, ErrorMessage = "User not found" }, null);
                }

                var relevantAchievements = user.Achievments.Where(ach => ach.Family == achievmentFamily).ToList();

                if (!relevantAchievements.Any())
                {
                    return (new BasicErrorResponse { Success = true }, $"{achievmentFamily}_0");
                }

                var maxAchievement = relevantAchievements
                    .OrderByDescending(ach =>
                    {
                        var parts = ach.StringId.Split('_');
                        return parts.Length > 1 && int.TryParse(parts[1], out int number) ? number : 0;
                    })
                    .FirstOrDefault();

                int currentMax = 0;
                if (maxAchievement != null)
                {
                    var parts = maxAchievement.StringId.Split('_');
                    if (parts.Length > 1 && int.TryParse(parts[1], out int number))
                    {
                        currentMax = number;
                    }
                }

                string currentAchievmentName = $"{achievmentFamily}_{currentMax + 1}";

                var doesAchievmentExist = await _context.Achievment.FirstOrDefaultAsync(a => a.StringId == currentAchievmentName);
                if(doesAchievmentExist == null) { return (new BasicErrorResponse() { Success = true }, null); }

                return (new BasicErrorResponse { Success = true }, currentAchievmentName);
            }
            catch (Exception ex)
            {
                return (new BasicErrorResponse { Success = false, ErrorMessage = $"Something went wrong: {ex.Message}" }, null);
            }
        }


        public async Task<(BasicErrorResponse error, AchievmentResponse? ach)> IncrementAchievmentProgress(string achievmentStringId, string userId)
        {
            try
            {
                AchievmentResponse achRes = new AchievmentResponse();

                var achievment = await _context.Achievment.FirstOrDefaultAsync(a => a.StringId == achievmentStringId);
                if (achievment == null) { return (new BasicErrorResponse() { Success = false, ErrorMessage = "Given achievments does not exists." }, null); }

                var userCount = await _context.AchievmentCounters.FirstOrDefaultAsync(a => a.UserId == userId && a.AchievmentId == achievment.Id);
                if (userCount == null)
                {
                    AchievmentCounters counter = new AchievmentCounters()
                    {
                        Counter = 1,
                        AchievmentId = achievment.Id,
                        UserId = userId,
                    };

                    await _context.AchievmentCounters.AddAsync(counter);

                    if (achievment.Threshold == 1)
                    {
                        achRes.ExceededThreshold = 1;
                        achRes.AchievmentEarnedName = achievment.Name;
                        achRes.AchievmentEarnedImage = achievment.Img;
                        achRes.GenerativeText = achievment.GenerativeText;

                        var user = await _context.Users.Include(u => u.Achievments).FirstOrDefaultAsync(u => u.Id == userId);
                        if (user != null)
                        {
                            if (user.Achievments == null)
                            {
                                user.Achievments = new List<Achievment>();
                            }
                            user.Achievments.Add(achievment);                            
                            await _context.SaveChangesAsync();
                        }

                        return (new BasicErrorResponse() { Success = true, }, achRes);
                    }

                    return (new BasicErrorResponse() { Success = true, }, null);
                }
                else
                {
                    userCount.Counter += 1;
                    await _context.SaveChangesAsync();

                    if (achievment.Threshold == userCount.Counter) 
                    {
                        achRes.ExceededThreshold = 1;
                        achRes.AchievmentEarnedName = achievment.Name;
                        achRes.AchievmentEarnedImage = achievment.Img;
                        achRes.GenerativeText = achievment.GenerativeText;

                        var user = await _context.Users.Include(u => u.Achievments).FirstOrDefaultAsync(u => u.Id == userId);
                        if (user != null)
                        {
                            if (user.Achievments == null)
                            {
                                user.Achievments = new List<Achievment>();
                            }
                            user.Achievments.Add(achievment);
                            await _context.SaveChangesAsync();
                        }

                        return (new BasicErrorResponse() { Success = true, }, achRes);
                    }

                    return (new BasicErrorResponse() { Success = true, }, null);
                }

            }
            catch (Exception ex) 
            { 
                return (new BasicErrorResponse() { Success = false, ErrorMessage = ex.Message }, null);
            }
        }
    }
}
