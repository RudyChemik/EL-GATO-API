using ElGato_API.VMO.ErrorResponse;
using MongoDB.Driver;

namespace ElGato_API.Interfaces
{
    public interface IHelperService
    {
        Task<T?> CreateMissingDoc<T>(string userId, IMongoCollection<T> collection) where T : class, new();
    }
}
