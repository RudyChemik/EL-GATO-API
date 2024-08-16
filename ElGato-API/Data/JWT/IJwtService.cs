using Microsoft.AspNetCore.Identity;

namespace ElGato_API.Data.JWT
{
    public interface IJwtService
    {
        string GetUserIdClaim();
        string GenerateJwtToken(IdentityUser user, string email, IList<string> roles);

    }
}
