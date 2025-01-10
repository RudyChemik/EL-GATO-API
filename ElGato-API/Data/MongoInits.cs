using ElGato_API.Models.User;
using ElGato_API.ModelsMongo.Diet;
using ElGato_API.ModelsMongo.History;
using ElGato_API.ModelsMongo.Training;
using MongoDB.Driver;

namespace ElGato_API.Data
{
    public class MongoInits : IMongoInits
    {
        private readonly IMongoCollection<DietDocument> _dietCollection;
        private readonly IMongoCollection<DailyTrainingDocument> _trainingCollection;
        private readonly IMongoCollection<ExercisesHistoryDocument> _exercisesHistoryCollection;
        public MongoInits(IMongoDatabase database) 
        {
            _dietCollection = database.GetCollection<DietDocument>("DailyDiet");
            _trainingCollection = database.GetCollection<DailyTrainingDocument>("DailyTraining");
            _exercisesHistoryCollection = database.GetCollection<ExercisesHistoryDocument>("ExercisesHistory");
        }
        public async Task CreateUserDietDocument(string userId) 
        {
            try
            {
                var existingDocument = await _dietCollection.Find(d => d.UserId == userId).FirstOrDefaultAsync();
                if (existingDocument == null)
                {
                    var newDoc = new DietDocument { UserId = userId, DailyPlans = new List<DailyDietPlan>() };
                    await _dietCollection.InsertOneAsync(newDoc);
                }
            }
            catch (Exception ex) 
            {
                throw new Exception("Error creating user diet document", ex);
            }
        }

        public async Task CreateUserExerciseHistoryDocument(string userId)
        {
            try
            {
                var existingDocument = await _exercisesHistoryCollection.Find(a=>a.UserId == userId).FirstOrDefaultAsync();
                if(existingDocument == null)
                {
                    var newDoc = new ExercisesHistoryDocument()
                    {
                        UserId = userId,
                        ExerciseHistoryLists = new List<ExerciseHistoryList>(),
                    };
                    await _exercisesHistoryCollection.InsertOneAsync(newDoc);
                }
            }
            catch(Exception ex)
            {
                throw new Exception("Error creating user diet document", ex);
            }
        }

        public async Task CreateUserTrainingDocument(string userId)
        {
            try
            {
                var existingDocument = await _trainingCollection.Find(a=>a.UserId == userId).FirstOrDefaultAsync();
                if(existingDocument == null)
                {
                    var newDoc = new DailyTrainingDocument { UserId = userId, Trainings = new List<DailyTrainingPlan>() };
                    await _trainingCollection.InsertOneAsync(newDoc);
                }
                
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating user training document", ex);
            }
        }
    }
}
