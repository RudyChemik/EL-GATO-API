using Microsoft.AspNetCore.Identity;

namespace ElGato_API.VMO.UserAuth
{
    public class LoginVMO
    {
        public IdentityResult IdentityResult { get; set; }
        public string JwtToken { get; set; }
    }
}
