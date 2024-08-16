using Microsoft.AspNetCore.Identity;

namespace ElGato_API.Models.User
{
    public class AppUser : IdentityUser
    {
        public string? Name { get; set; }
        public bool Metric { get; set; } = true;
        public UserInformation? UserInformation { get; set; }
        public CalorieInformation? CalorieInformation { get; set; }
    }
}
