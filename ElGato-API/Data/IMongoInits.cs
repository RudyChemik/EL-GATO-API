namespace ElGato_API.Data
{
    public interface IMongoInits
    {
        Task CreateUserDietDocument(string userId);
    }
}
