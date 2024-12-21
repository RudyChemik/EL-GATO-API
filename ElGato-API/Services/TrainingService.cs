using ElGato_API.Data;
using ElGato_API.Interfaces;
using ElGato_API.Models.Training;
using ElGato_API.ModelsMongo.Diet.History;
using ElGato_API.ModelsMongo.Meal;
using ElGato_API.ModelsMongo.Training;
using ElGato_API.VMO.ErrorResponse;
using ElGato_API.VMO.Training;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace ElGato_API.Services
{
    public class TrainingService : ITrainingService
    {
        private readonly AppDbContext _context;
        private readonly IMongoCollection<DailyTrainingDocument> _trainingCollection;
        private readonly IMongoCollection<TrainingHistoryDocument> _trainingHistoryCollection;
        public TrainingService(IMongoDatabase database, AppDbContext context) 
        {
            _trainingCollection = database.GetCollection<DailyTrainingDocument>("DailyTraining");
            _trainingHistoryCollection = database.GetCollection<TrainingHistoryDocument>("TrainingHistory");
            _context = context;
        }

        public async Task<(BasicErrorResponse error, List<ExerciseVMO>? data)> GetAllExercises()
        {
            try
            {
                var exercises = await _context.Exercises.Include(e => e.MusclesEngaded).ToListAsync();

                var response = exercises.Select(e => new ExerciseVMO
                {
                    Id = e.Id,
                    Name = e.Name,
                    Desc = e.Desc,
                    Image = e.Image,
                    ImgGifPart = e.ImageGifPart,
                    MusclesEngaged = e.MusclesEngaded.Select(m => new MuscleVMO
                    {
                        Id = m.Id,
                        Name = m.Name,
                        NormalName = m.NormalName,
                        Group = m.Group.ToString()
                    }).ToList(),
                    MainBodyPart = e.MainBodyPart.ToString(),
                    SpecificBodyPart = e.SpecificBodyPart.ToString(),
                    Equipment = e.Equipment.ToString(),
                    Difficulty = e.Difficulty.ToString()
                }).ToList();

                return (new BasicErrorResponse() { ErrorCode = ErrorCodes.None, ErrorMessage = "Sucessfully retrived exercise data.", Success = true }, response);
            }
            catch (Exception ex) 
            {
                return (new BasicErrorResponse() { ErrorCode = ErrorCodes.Internal, ErrorMessage = $"Internal, {ex.Message}", Success = false }, null);
            }
        }
    }
}
