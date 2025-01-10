namespace ElGato_API.Data
{
    public interface IMongoInits
    {
        Task CreateUserDietDocument(string userId);
        Task CreateUserTrainingDocument(string userId);
        Task CreateUserExerciseHistoryDocument(string userId);
    }
}
