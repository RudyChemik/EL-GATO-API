using ElGato_API.Interfaces;
using MongoDB.Driver;

namespace ElGato_API.Services
{
    public class HelperService : IHelperService
    {
        private readonly ILogger<HelperService> _logger;
        public HelperService(ILogger<HelperService> logger)
        {
            _logger = logger;
        }

        public async Task<T?> CreateMissingDoc<T>(string userId, IMongoCollection<T> collection) where T : class, new()
        {
            try
            {
                _logger.LogInformation($"Trying to create new doc for user. UserId: {userId} Type: {typeof(T)} Method: {nameof(CreateMissingDoc)}");
                T newDoc = new T();

                var userIdProp = typeof(T).GetProperty("UserId");
                if (userIdProp == null || userIdProp.PropertyType != typeof(string) || !userIdProp.CanWrite)
                {
                    _logger.LogCritical($"Failed to create new document for user. UserId: {userId} Type: {typeof(T)} Method: {nameof(CreateMissingDoc)}");
                    return null;
                }

                userIdProp.SetValue(newDoc, userId);

                await collection.InsertOneAsync(newDoc);
                _logger.LogInformation($"Sucesfully create new document of type {typeof(T)} for user {userId}");
                return newDoc;

            }
            catch(Exception ex)
            {
                _logger.LogCritical(ex, $"Helper doc insertion failed. UserId: {userId} Type: {typeof(T)} Method: {nameof(CreateMissingDoc)}");
                return null;
            }
        }
    }
}
