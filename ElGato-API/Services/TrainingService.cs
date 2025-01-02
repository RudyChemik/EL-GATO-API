using ElGato_API.Data;
using ElGato_API.Interfaces;
using ElGato_API.Models.Training;
using ElGato_API.ModelsMongo.Diet.History;
using ElGato_API.ModelsMongo.Meal;
using ElGato_API.ModelsMongo.Training;
using ElGato_API.VM.Training;
using ElGato_API.VMO.ErrorResponse;
using ElGato_API.VMO.Training;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace ElGato_API.Services
{
    public class TrainingService : ITrainingService
    {
        private readonly AppDbContext _context;
        private readonly IMongoCollection<DailyTrainingDocument> _trainingCollection;
        private readonly IMongoCollection<TrainingHistoryDocument> _trainingHistoryCollection;
        private readonly IMongoCollection<LikedExercisesDocument> _trainingLikesCollection;
        public TrainingService(IMongoDatabase database, AppDbContext context) 
        {
            _trainingCollection = database.GetCollection<DailyTrainingDocument>("DailyTraining");
            _trainingHistoryCollection = database.GetCollection<TrainingHistoryDocument>("TrainingHistory");
            _trainingLikesCollection = database.GetCollection<LikedExercisesDocument>("LikedExercises");
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

        public async Task<BasicErrorResponse> LikeExercise(string userId, LikeExerciseVM model)
        {
            try
            {

                var existingDoc = await _trainingLikesCollection.Find(a => a.UserId == userId).FirstOrDefaultAsync();
                if (existingDoc == null)
                {
                    LikedExercisesDocument doc = new LikedExercisesDocument()
                    {
                        UserId = userId,
                    };

                    if (model.Own)
                    {
                        doc.Own = new List<string>() { model.Name };
                        doc.Premade = new List<LikedExercise>();
                    }
                    else
                    {
                        var existingEx = await _context.Exercises.FirstOrDefaultAsync(a=>a.Id == model.Id);
                        if (existingEx == null) { return new BasicErrorResponse() { ErrorCode = ErrorCodes.NotFound, ErrorMessage = "given premade exercise not found", Success = false }; }
                        
                        doc.Own = new List<string>();
                        doc.Premade = new List<LikedExercise>() { new LikedExercise() { Name = model.Name, Id = model.Id ?? existingEx.Id } };
                    }                

                    await _trainingLikesCollection.InsertOneAsync(doc);
                    return new BasicErrorResponse() { ErrorCode = ErrorCodes.None, Success = true };
                }

                if(model.Own)
                {
                    var alreadyExist = existingDoc.Own.FirstOrDefault(model.Name);
                    if (alreadyExist != null) 
                    {
                        return new BasicErrorResponse() { ErrorCode = ErrorCodes.AlreadyExists, ErrorMessage = "own exercise with given name already saved", Success = false };
                    }

                    existingDoc.Own.Add(model.Name);
                }
                else
                {
                    var existingEx = await _context.Exercises.FirstOrDefaultAsync(a => a.Id == model.Id);
                    if (existingEx == null) { return new BasicErrorResponse() { ErrorCode = ErrorCodes.NotFound, ErrorMessage = "given premade exercise not found", Success = false }; }

                    var alreadyExists = existingDoc.Premade.FirstOrDefault(a => a.Id == model.Id);
                    if (alreadyExists != null) 
                    {
                        return new BasicErrorResponse() { ErrorCode = ErrorCodes.AlreadyExists, ErrorMessage = "premade exercise with given name already saved", Success = false };
                    }

                    existingDoc.Premade.Add(new LikedExercise { Name = model.Name, Id = model.Id ?? existingEx.Id });
                }

                await _trainingLikesCollection.ReplaceOneAsync(a => a.UserId == userId, existingDoc);

                return new BasicErrorResponse() { ErrorCode = ErrorCodes.None, Success = true };
            }
            catch (Exception ex) 
            {
                return new BasicErrorResponse() { ErrorCode = ErrorCodes.Internal, ErrorMessage = $"Internal server error {ex.Message}", Success = false };
            }
        }

        public async Task<BasicErrorResponse> RemoveExerciseFromLiked(string userId, LikeExerciseVM model)
        {
            try
            {
                var existingDoc = await _trainingLikesCollection.Find(a => a.UserId == userId).FirstOrDefaultAsync();
                if (existingDoc == null) { return new BasicErrorResponse() { ErrorCode = ErrorCodes.NotFound, ErrorMessage = "User liked exercise document not found.", Success = false }; }

                if (model.Own)
                {
                    var exerciseToRemove = existingDoc.Own.FirstOrDefault(model.Name);
                    if (exerciseToRemove == null)
                    {
                        return new BasicErrorResponse() { ErrorCode = ErrorCodes.NotFound, ErrorMessage = "Given own exercise not found", Success = false };
                    }

                    existingDoc.Own.Remove(exerciseToRemove);
                }
                else
                {
                   var exerciseToRemove = existingDoc.Premade.FirstOrDefault(a => a.Id == model.Id);
                    if (exerciseToRemove == null)
                    {
                        return new BasicErrorResponse() { ErrorCode = ErrorCodes.NotFound, ErrorMessage = "Given premade exercise not found", Success = false };
                    }

                    existingDoc.Premade.Remove(exerciseToRemove);
                }

                await _trainingLikesCollection.ReplaceOneAsync(a => a.UserId == userId, existingDoc);
                return new BasicErrorResponse() { ErrorCode = ErrorCodes.None, Success = true };
            }
            catch (Exception ex) 
            {
                return new BasicErrorResponse() { ErrorCode = ErrorCodes.Internal, ErrorMessage = $"Internal server error {ex.Message}", Success = false };
            }
        }


    }
}
