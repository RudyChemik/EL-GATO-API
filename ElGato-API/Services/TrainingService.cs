using ElGato_API.Data;
using ElGato_API.Interfaces;
using ElGato_API.Models.Training;
using ElGato_API.ModelsMongo.Diet;
using ElGato_API.ModelsMongo.History;
using ElGato_API.ModelsMongo.Meal;
using ElGato_API.ModelsMongo.Training;
using ElGato_API.VM.Training;
using ElGato_API.VMO.ErrorResponse;
using ElGato_API.VMO.Training;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ElGato_API.Services
{
    public class TrainingService : ITrainingService
    {
        private readonly AppDbContext _context;
        private readonly IMongoCollection<DailyTrainingDocument> _trainingCollection;
        private readonly IMongoCollection<TrainingHistoryDocument> _trainingHistoryCollection;
        private readonly IMongoCollection<LikedExercisesDocument> _trainingLikesCollection;
        private readonly IMongoCollection<ExercisesHistoryDocument> _exercisesHistoryCollection;
        public TrainingService(IMongoDatabase database, AppDbContext context) 
        {
            _trainingCollection = database.GetCollection<DailyTrainingDocument>("DailyTraining");
            _trainingHistoryCollection = database.GetCollection<TrainingHistoryDocument>("TrainingHistory");
            _trainingLikesCollection = database.GetCollection<LikedExercisesDocument>("LikedExercises");
            _exercisesHistoryCollection = database.GetCollection<ExercisesHistoryDocument>("ExercisesHistory");
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

        public async Task<(BasicErrorResponse error, List<LikedExercisesVMO>? data)> GetAllLikedExercises(string userId)
        {
            try
            {
                List<LikedExercisesVMO> likedExercises = new List<LikedExercisesVMO>();

                var userLikesDoc = await _trainingLikesCollection.Find(a => a.UserId == userId).FirstOrDefaultAsync();
                if (userLikesDoc == null)
                {
                    LikedExercisesDocument doc = new LikedExercisesDocument()
                    {
                        UserId = userId,
                        Own = new List<string>(),
                        Premade = new List<LikedExercise>()
                    };

                    await _trainingLikesCollection.InsertOneAsync(doc);
                    return (new BasicErrorResponse() { ErrorCode = ErrorCodes.None, Success = true }, likedExercises);
                }

                foreach(var exercie in userLikesDoc.Own)
                {
                    likedExercises.Add(new LikedExercisesVMO() { Name = exercie, Own = true });
                }

                foreach(var exercise in userLikesDoc.Premade)
                {
                    likedExercises.Add(new LikedExercisesVMO() { Name = exercise.Name, Id = exercise.Id, Own = false });
                }

                return (new BasicErrorResponse() { ErrorCode = ErrorCodes.None, Success = true }, likedExercises);
            }
            catch (Exception ex) 
            {
                return (new BasicErrorResponse() { ErrorCode = ErrorCodes.Internal, ErrorMessage = $"Something went wrong, {ex.Message}", Success = false }, null);
            }
        }

        public async Task<(BasicErrorResponse error, TrainingDayVMO? data)> GetUserTrainingDay(string userId, DateTime date)
        {
            try
            {
                var userTrainingDocument = await _trainingCollection.Find(a=>a.UserId == userId).FirstOrDefaultAsync();
                if(userTrainingDocument == null) { return (new BasicErrorResponse() { ErrorCode = ErrorCodes.NotFound, ErrorMessage = "User training document not found", Success = false }, null); }
             
                var targetedPlan = userTrainingDocument.Trainings.FirstOrDefault(a=>a.Date == date);
                if(targetedPlan != null)
                {
                    List<TrainingDayExerciseVMO> modelList = new List<TrainingDayExerciseVMO>();
                    foreach (var ex in targetedPlan.Exercises)
                    {
                        var pastData = await GetPastDataFromExercise(userId, date, ex.Name);

                        TrainingDayExerciseVMO modelExercise = new TrainingDayExerciseVMO()
                        {
                            Exercise = ex,
                            PastData = pastData,
                        };

                        modelList.Add(modelExercise);
                    }

                    TrainingDayVMO data = new TrainingDayVMO()
                    {
                        Date = date,
                        Exercises = modelList,
                    };

                    return (new BasicErrorResponse() { ErrorCode = ErrorCodes.None, Success = true}, data);
                }

                if(userTrainingDocument.Trainings != null && userTrainingDocument.Trainings.Count() >= 7)
                {
                    var oldestTraining = userTrainingDocument.Trainings.OrderBy(dp => dp.Date).First();
                    await MoveTrainingToHistory(userId, oldestTraining);

                    var update = Builders<DailyTrainingDocument>.Update.PullFilter(d => d.Trainings, dp => dp.Date == oldestTraining.Date);
                    await _trainingCollection.UpdateOneAsync(d => d.UserId == userId, update);
                }

                DailyTrainingPlan trainingUpd = new DailyTrainingPlan()
                {
                    Date = date,
                    Exercises = new List<DailyExercise>(),
                };

                var updated = Builders<DailyTrainingDocument>.Update.Push(d => d.Trainings, trainingUpd);
                await _trainingCollection.UpdateOneAsync(d => d.UserId == userId, updated);

                return (new BasicErrorResponse() { Success = true, ErrorCode = ErrorCodes.None }, new TrainingDayVMO() { Date = date, Exercises = new List<TrainingDayExerciseVMO>()});
            }
            catch(Exception ex)
            {
                return (new BasicErrorResponse() { ErrorCode = ErrorCodes.Internal, ErrorMessage = $"Something went wrong, {ex.Message}", Success = false }, null);
            }
        }

        public async Task<BasicErrorResponse> AddExercisesToTrainingDay(string userId, AddExerciseToTrainingVM model)
        {
            try
            {
                var userTrainingDocument = await _trainingCollection.Find(a=>a.UserId == userId).FirstOrDefaultAsync();
                if (userTrainingDocument == null) { return new BasicErrorResponse() { Success = false, ErrorCode = ErrorCodes.NotFound, ErrorMessage = "User daily training document not found, couldnt perform any action." }; }

                var targetedPlan = userTrainingDocument.Trainings.FirstOrDefault(a => a.Date == model.Date);
                if (targetedPlan == null)
                {
                    return new BasicErrorResponse() { Success = false, ErrorCode = ErrorCodes.NotFound, ErrorMessage = "couldn''y find any matching dates in training document, proces terminated" };
                }

                int lastId = 0;
                if (targetedPlan.Exercises != null && targetedPlan.Exercises.Count() > 0)
                {
                    lastId = targetedPlan.Exercises[targetedPlan.Exercises.Count() - 1].PublicId + 1;
                }

                List<DailyExercise> listOfExercisesForInsertion = new List<DailyExercise>();

                foreach(var ex in model.Name)
                {
                    DailyExercise daileEx = new DailyExercise()
                    {
                        Name = ex,
                        PublicId = lastId,
                        Series = new List<ExerciseSeries>() { new ExerciseSeries() { PublicId = 1, Repetitions = 0, WeightKg = 0, WeightLbs = 0, Tempo = new ExerciseSerieTempo() { } } },
                    };

                    listOfExercisesForInsertion.Add(daileEx);
                    lastId++;
                }

                targetedPlan.Exercises.AddRange(listOfExercisesForInsertion);

                await _trainingCollection.ReplaceOneAsync(a=>a.UserId == userId, userTrainingDocument);

                return new BasicErrorResponse() { Success = true, ErrorCode = ErrorCodes.None };
            }
            catch (Exception ex)
            {
                return new BasicErrorResponse() { ErrorCode = ErrorCodes.Internal, ErrorMessage = $"an error occurec {ex.Message}", Success = false };
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
    

        public async Task<BasicErrorResponse> RemoveExercisesFromLiked(string userId, List<LikeExerciseVM> model)
        {
            try 
            {
                var existingDoc = await _trainingLikesCollection.Find(a => a.UserId == userId).FirstOrDefaultAsync();
                if (existingDoc == null) { return new BasicErrorResponse() { ErrorCode = ErrorCodes.NotFound, ErrorMessage = "User liked exercise document not found.", Success = false }; }

                foreach (var exercise in model) 
                {
                    if (exercise.Own)
                    {
                        var exerciseToRemove = existingDoc.Own.FirstOrDefault(e => e == exercise.Name);
                        if (exerciseToRemove == null)
                        {
                            return new BasicErrorResponse() { ErrorCode = ErrorCodes.NotFound, ErrorMessage = $"Given own exercise not found {exercise.Name}", Success = false };
                        }

                        existingDoc.Own.Remove(exerciseToRemove);
                    }
                    else
                    {
                        var exerciseToRemove = existingDoc.Premade.FirstOrDefault(a => a.Id == exercise.Id);
                        if (exerciseToRemove == null)
                        {
                            return new BasicErrorResponse() { ErrorCode = ErrorCodes.NotFound, ErrorMessage = $"Given premade exercise not found {exercise.Name},{exercise.Id}", Success = false };
                        }

                        existingDoc.Premade.Remove(exerciseToRemove);
                    }
                }

                await _trainingLikesCollection.ReplaceOneAsync(a => a.UserId == userId, existingDoc);
                return new BasicErrorResponse() { ErrorCode = ErrorCodes.None, Success = true };
            }
            catch (Exception ex)
            {
                return new BasicErrorResponse() { ErrorCode = ErrorCodes.Internal, ErrorMessage = $"Internal server error {ex.Message}", Success = false };
            }
        }


        private async Task MoveTrainingToHistory(string userId, DailyTrainingPlan oldestPlan)
        {
            var trainingHistoryDoc = await _trainingHistoryCollection.Find(a=>a.UserId == userId).FirstOrDefaultAsync();
            if (trainingHistoryDoc == null)
            {
                TrainingHistoryDocument historyDoc = new TrainingHistoryDocument()
                {
                    UserId = userId,
                    DailyTrainingPlans = new List<DailyTrainingPlan> { oldestPlan }
                };

                await _trainingHistoryCollection.InsertOneAsync(historyDoc);
                return;
            }

            var update = Builders<TrainingHistoryDocument>.Update.Push(h => h.DailyTrainingPlans, oldestPlan);
            await _trainingHistoryCollection.UpdateOneAsync(h => h.UserId == userId, update);
        }

        private async Task<PastExerciseData?> GetPastDataFromExercise(string userId, DateTime currentDate, string exerciseName)
        {
            PastExerciseData data = null;

            var filter = Builders<ExercisesHistoryDocument>.Filter.And(
                Builders<ExercisesHistoryDocument>.Filter.Eq(e => e.UserId, userId),
                Builders<ExercisesHistoryDocument>.Filter.ElemMatch(
                    e => e.ExerciseHistoryLists,
                    eh => eh.ExerciseName == exerciseName
                )
            );

            var projection = Builders<ExercisesHistoryDocument>.Projection.Expression(doc =>
                doc.ExerciseHistoryLists
                    .Where(eh => eh.ExerciseName == exerciseName)
                    .SelectMany(eh => eh.ExerciseData)
                    .Where(ed => ed.Date < currentDate)
                    .OrderByDescending(ed => ed.Date)
                    .FirstOrDefault()
            );

            var res = await _exercisesHistoryCollection
                .Find(filter)
                .Project(projection)
                .FirstOrDefaultAsync();

            if(res != null)
            {
                data = new PastExerciseData();
                data.Series = res.Series??new List<ExerciseSeries>();
                data.Date = res.Date;
            }


            return data;
        }

        public async Task<BasicErrorResponse> WriteSeriesForAnExercise(string userId, AddSeriesToAnExerciseVM model)
        {
            try
            {
                if (!model.Series.Any())
                {
                    model.Series.Add(new AddSeriesVM()
                    {
                        Repetitions = 0,
                        WeightKg = 0,
                        WeightLbs = 0,
                    });
                }

                foreach (var series in model.Series)
                {
                    if (series.WeightKg == 0)
                    {
                        series.WeightKg = (series.WeightLbs / 2.20462);
                    }
                    else if (series.WeightLbs == 0)
                    {
                        series.WeightLbs = (series.WeightKg * 2.20462);
                    }
                }

                var filter = Builders<DailyTrainingDocument>.Filter.And(
                    Builders<DailyTrainingDocument>.Filter.Eq(t => t.UserId, userId),
                    Builders<DailyTrainingDocument>.Filter.ElemMatch(t => t.Trainings, training => training.Date == model.Date)
                );

                var trainingDocument = await _trainingCollection.Find(filter).FirstOrDefaultAsync();
                if (trainingDocument == null)
                {
                    return new BasicErrorResponse
                    {
                        Success = false,
                        ErrorMessage = "Training document not found for the given user",
                        ErrorCode = ErrorCodes.NotFound
                    };
                }

                var trainingIndex = trainingDocument.Trainings.FindIndex(t => t.Date == model.Date);
                if (trainingIndex == -1)
                {
                    return new BasicErrorResponse
                    {
                        Success = false,
                        ErrorMessage = "Training session not found for the given date",
                        ErrorCode = ErrorCodes.NotFound
                    };
                }

                var exerciseIndex = trainingDocument.Trainings[trainingIndex].Exercises.FindIndex(e => e.PublicId == model.PublicId);
                if (exerciseIndex == -1)
                {
                    return new BasicErrorResponse
                    {
                        Success = false,
                        ErrorMessage = "Exercise not found in the training session",
                        ErrorCode = ErrorCodes.NotFound
                    };
                }

                int highestPublicId = trainingDocument.Trainings[trainingIndex].Exercises[exerciseIndex].Series.Any()
                    ? trainingDocument.Trainings[trainingIndex].Exercises[exerciseIndex].Series.Max(s => s.PublicId)
                    : 0;

                var newSeriesList = model.Series.Select((series, index) => new ExerciseSeries
                {
                    PublicId = highestPublicId + index + 1,
                    Repetitions = series.Repetitions,
                    WeightKg = series.WeightKg,
                    WeightLbs = series.WeightLbs
                }).ToList();

                var updateFilter = Builders<DailyTrainingDocument>.Filter.And(
                    Builders<DailyTrainingDocument>.Filter.Eq(t => t.UserId, userId),
                    Builders<DailyTrainingDocument>.Filter.Eq($"Trainings.{trainingIndex}.Exercises.{exerciseIndex}.PublicId", model.PublicId)
                );

                var update = Builders<DailyTrainingDocument>.Update.PushEach($"Trainings.{trainingIndex}.Exercises.{exerciseIndex}.Series", newSeriesList);

                var result = await _trainingCollection.UpdateOneAsync(updateFilter, update);

                if (result.ModifiedCount == 0)
                {
                    return new BasicErrorResponse
                    {
                        Success = false,
                        ErrorMessage = "Failed to add series to exercise",
                        ErrorCode = ErrorCodes.Failed
                    };
                }

                return new BasicErrorResponse
                {
                    Success = true,
                    ErrorCode = ErrorCodes.None
                };
            }
            catch (Exception ex)
            {
                return new BasicErrorResponse
                {
                    Success = false,
                    ErrorMessage = $"An internal server error occurred {ex.Message}",
                    ErrorCode = ErrorCodes.Internal
                };
            }
        }

        public async Task<BasicErrorResponse> UpdateExerciseHistory(string userId, HistoryUpdateVM model, DateTime date)
        {
            try
            {
                var userHistoryDocument = await _exercisesHistoryCollection.Find(a=>a.UserId == userId).FirstOrDefaultAsync();
                if (userHistoryDocument == null)
                {
                    ExercisesHistoryDocument doc = new ExercisesHistoryDocument()
                    {
                        UserId = userId,
                        ExerciseHistoryLists = new List<ExerciseHistoryList>()
                        {
                            new ExerciseHistoryList()
                            {
                                ExerciseName = model.ExerciseName,
                                ExerciseData = new List<ExerciseData>()
                                {
                                    model.ExerciseData,
                                }
                            }
                            
                        }
                    };

                    await _exercisesHistoryCollection.InsertOneAsync(doc);
                    return new BasicErrorResponse() { Success = true, ErrorCode = ErrorCodes.None, ErrorMessage = $"Success" }; 
                }

                var givenExercisePastData = userHistoryDocument.ExerciseHistoryLists.FirstOrDefault(a=>a.ExerciseName == model.ExerciseName);
                if(givenExercisePastData == null)
                {
                    ExerciseHistoryList newRecord = new ExerciseHistoryList()
                    {
                        ExerciseName= model.ExerciseName,
                        ExerciseData= new List<ExerciseData>() { model.ExerciseData }
                    };

                    userHistoryDocument.ExerciseHistoryLists.Add(newRecord);

                    var filter = Builders<ExercisesHistoryDocument>.Filter.Eq(a => a.UserId, userId);
                    var update = Builders<ExercisesHistoryDocument>.Update.Set(a => a.ExerciseHistoryLists, userHistoryDocument.ExerciseHistoryLists);

                    await _exercisesHistoryCollection.UpdateOneAsync(filter, update);

                    return new BasicErrorResponse() { Success = true, ErrorCode = ErrorCodes.None, ErrorMessage = "Success" };
                }

                var exercisePastDay = givenExercisePastData.ExerciseData.FirstOrDefault(a => a.Date == date);
                if (exercisePastDay == null)
                {
                    givenExercisePastData.ExerciseData.Add(model.ExerciseData);
                }
                else
                {
                    exercisePastDay.Series = model.ExerciseData.Series;
                }

                var updateFilter = Builders<ExercisesHistoryDocument>.Filter.Eq(a => a.UserId, userId);
                var updateDefinition = Builders<ExercisesHistoryDocument>.Update.Set(a => a.ExerciseHistoryLists, userHistoryDocument.ExerciseHistoryLists);

                await _exercisesHistoryCollection.UpdateOneAsync(updateFilter, updateDefinition);

                return new BasicErrorResponse { Success = true };
            }
            catch (Exception ex)
            {
                return new BasicErrorResponse
                {
                    Success = false,
                    ErrorMessage = $"An internal server error occurred {ex.Message}",
                    ErrorCode = ErrorCodes.Internal
                };
            }
        }

        public async Task<BasicErrorResponse> RemoveSeriesFromAnExercise(string userId, RemoveSeriesFromExerciseVM model)
        {
            try
            {
                var trainingDocument = await _trainingCollection.Find(a => a.UserId == userId).FirstOrDefaultAsync();
                if (trainingDocument == null) { return new BasicErrorResponse() { ErrorCode = ErrorCodes.NotFound, ErrorMessage = "user training document not found.", Success = false }; }

                if (trainingDocument.Trainings == null) { return new BasicErrorResponse() { Success = false, ErrorCode = ErrorCodes.Failed, ErrorMessage = "Could not remove any - daily training doc empty." }; }

                var targetedDay = trainingDocument.Trainings.FirstOrDefault(a => a.Date == model.Date);
                if (targetedDay == null) { return new BasicErrorResponse() { Success = false, ErrorCode = ErrorCodes.NotFound, ErrorMessage = "Current exercise day does not exist." }; }

                var targetExercise = targetedDay.Exercises.FirstOrDefault(a => a.PublicId == model.ExercisePublicId);
                if (targetExercise == null) { return new BasicErrorResponse() { ErrorCode = ErrorCodes.NotFound, ErrorMessage = "Exercise not found. Couldnt perform remove operation.", Success = false }; }

                foreach (var idToRemove in model.seriesIdToRemove)
                {
                    var serieToRemove = targetExercise.Series.FirstOrDefault(a => a.PublicId == idToRemove);
                    if (serieToRemove != null)
                    {
                        targetExercise.Series.Remove(serieToRemove);
                    }
                }

                var updateResult = await _trainingCollection.ReplaceOneAsync(doc => doc.UserId == userId, trainingDocument);

                if (!updateResult.IsAcknowledged || updateResult.ModifiedCount == 0)
                {
                    return new BasicErrorResponse() { ErrorCode = ErrorCodes.Failed, ErrorMessage = $"Failed while performing the update", Success = false };
                }

                return new BasicErrorResponse { Success = true };
            }
            catch(Exception ex)
            {
                return new BasicErrorResponse() { Success = false, ErrorCode = ErrorCodes.Internal, ErrorMessage = $"{ex.Message}" };
            }
        }
    }
}
