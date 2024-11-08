using ElGato_API.Models.Requests;
using Microsoft.AspNetCore.Identity;

namespace ElGato_API.Models.User
{
    public class AppUser : IdentityUser
    {
        public string? Name { get; set; }
        public bool Metric { get; set; } = true;
        public string Pfp { get; set; } = "/pfp-images/e2f56642-a493-4c6d-924b-d3072714646a.png";
        public UserInformation? UserInformation { get; set; }
        public CalorieInformation? CalorieInformation { get; set; }
        public List<Achievment>? Achievments { get; set; } = new List<Achievment>();
        public List<AchievmentCounters> AchivmentCounter { get; set; } = new List<AchievmentCounters>();
    }
}
