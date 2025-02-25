using ElGato_API.Data;
using ElGato_API.Interfaces;
using ElGato_API.ModelsMongo.History;
using ElGato_API.VMO.ErrorResponse;
using ElGato_API.VMO.User;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace ElGato_API.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<UserService> _logger;
        private readonly IMongoCollection<ExercisesHistoryDocument> _exercisesHistoryCollection;
        private readonly IHelperService _helperService;
        public UserService(AppDbContext dbContext, ILogger<UserService> logger, IMongoDatabase database, IHelperService helperService) 
        { 
            _dbContext = dbContext;
            _logger = logger;
            _exercisesHistoryCollection = database.GetCollection<ExercisesHistoryDocument>("ExercisesHistory");
            _helperService = helperService;
        }

        public async Task<(BasicErrorResponse error, string? data)> GetSystem(string userId)
        {
            try
            {
                var res = await _dbContext.AppUser.FirstOrDefaultAsync(a=>a.Id == userId);
                if(res == null)
                {
                    return (new BasicErrorResponse() { ErrorCode = ErrorCodes.NotFound, ErrorMessage = $"user with specified id not found", Success = false }, null);
                }

                return (new BasicErrorResponse() { Success = true, ErrorCode = ErrorCodes.None }, res.Metric ? "metric" : "imperial");
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, $"Failed while trying to get system. UserId: {userId} Method: {nameof(GetSystem)}");
                return (new BasicErrorResponse() { ErrorCode = ErrorCodes.Internal, ErrorMessage = $"something went wrong, {ex.Message} ", Success = false}, null);
            }            
        }

        public async Task<(BasicErrorResponse error, UserCalorieIntake model)> GetUserCalories(string userId)
        {
            BasicErrorResponse error = new BasicErrorResponse() { Success = false };
            UserCalorieIntake userCalorieIntake = new UserCalorieIntake();

            try
            {
                var res = await _dbContext.Users.Include(x=>x.CalorieInformation).FirstOrDefaultAsync(a=>a.Id == userId);
                if (res == null || res.CalorieInformation == null) 
                {
                    error.ErrorMessage = "User calorie intake information not found.";
                    error.ErrorCode = ErrorCodes.NotFound;
                    return (error, userCalorieIntake);
                }

                userCalorieIntake.Kcal = res.CalorieInformation.Kcal;
                userCalorieIntake.Carbs = res.CalorieInformation.Carbs;
                userCalorieIntake.Fats = res.CalorieInformation.Fat;
                userCalorieIntake.Protein = res.CalorieInformation.Protein;

                error.Success = true;
                error.ErrorCode = ErrorCodes.None;
                return (error, userCalorieIntake);
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, $"Failed while trying to get user calories intake. UserId: {userId} Method: {nameof(GetUserCalories)}");
                error.ErrorMessage = ex.Message;
                error.ErrorCode = ErrorCodes.Internal;
                return (error, userCalorieIntake);
            }
        }

        public async Task<(BasicErrorResponse error, ExercisePastDataVMO? data)> GetPastExerciseData(string userId, string exerciseName, string period = "all")
        {
            try
            {
                if (period != "all" && period != "year" && period != "month" && period != "week")
                {
                    _logger.LogWarning($"User tried to use diffrent period than expected. UserId {userId} PeriodUsed: {period} Method: {nameof(ExercisePastDataVMO)}");
                    return (new BasicErrorResponse() { ErrorCode = ErrorCodes.ModelStateNotValid, ErrorMessage = $"Invalid period: {period}. Allowed values are 'all', 'year', 'month', 'week'.", Success = false}, null);
                }

                var userPastExercisesDoc = await _exercisesHistoryCollection.Find(a=>a.UserId == userId).FirstOrDefaultAsync();
                if(userPastExercisesDoc == null)
                {
                    _logger.LogWarning($"User past exercise document not found. UserId: {userId} Method: {nameof(GetPastExerciseData)}");

                    var newDoc = await _helperService.CreateMissingDoc(userId, _exercisesHistoryCollection);
                    if(newDoc == null)
                    {
                        return (new BasicErrorResponse() { Success = false, ErrorCode = ErrorCodes.NotFound, ErrorMessage = "User exercise history document not found." }, null);
                    }

                    return (new BasicErrorResponse() { Success = true, ErrorCode = ErrorCodes.None, ErrorMessage = $"Correctly retrived {exerciseName} data but document is empty." }, new ExercisePastDataVMO() { ExerciseName = exerciseName});
                }

                var targetedExercise = userPastExercisesDoc.ExerciseHistoryLists.FirstOrDefault(a=>a.ExerciseName == exerciseName);
                if(targetedExercise == null)
                {
                    return (new BasicErrorResponse() { Success = true, ErrorCode = ErrorCodes.None, ErrorMessage = $"No past data for {exerciseName} found."}, new ExercisePastDataVMO() { ExerciseName = exerciseName});
                }

                ExercisePastDataVMO exercisePastDataVMO = new ExercisePastDataVMO() { ExerciseName = exerciseName};
                var exData = targetedExercise.ExerciseData;

                switch (period)
                {
                    case "year":
                        exData = targetedExercise.ExerciseData.Where(a => a.Date >= DateTime.Now.AddYears(-1)).ToList();
                        break;
                    case "month":
                        exData = targetedExercise.ExerciseData.Where(a => a.Date >= DateTime.Now.AddMonths(-1)).ToList();
                        break;
                    case "week":
                        exData = targetedExercise.ExerciseData.Where(a => a.Date >= DateTime.Now.AddDays(-7)).ToList();
                        break;
                }

                exercisePastDataVMO.PastData.AddRange(
                    exData.Select(a => new ExercisePastData
                    {
                        Date = a.Date,
                        Series = a.Series.Select(serie => new ExercisePastSerieData
                        {
                            Repetitions = serie.Repetitions,
                            WeightKg = serie.WeightKg,
                            WeightLbs = serie.WeightLbs
                        }).ToList()
                    })
                );

                return (new BasicErrorResponse() { ErrorCode = ErrorCodes.None, Success = true, ErrorMessage = "Sucessfully retrived data." }, exercisePastDataVMO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed while trying to get past data of an exercise. UserId: {userId} ExerciseName: {exerciseName} Method: {nameof(GetPastExerciseData)}");
                return (new BasicErrorResponse() { ErrorCode = ErrorCodes.Internal, ErrorMessage = $"Error occured: {ex.Message}", Success = false }, null);
            }
        }
    }
}
