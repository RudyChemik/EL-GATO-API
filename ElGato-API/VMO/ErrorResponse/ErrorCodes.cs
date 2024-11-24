using System.Text.Json.Serialization;

namespace ElGato_API.VMO.ErrorResponse
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ErrorCodes
    {
        None,
        AlreadyExists
    }

}
