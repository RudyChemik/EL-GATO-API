using System.ComponentModel.DataAnnotations;

namespace ElGato_API.Models.Requests
{
    public class ReportedMeals
    {
        [Key]
        public int Id { get; set; }
        public string MealId { get; set; }
        public string MealName { get; set; }
        public string UserId { get; set; }
        public int Cause { get; set; }
        public bool Resolved { get; set; } = false;
        public string? ResolvedById { get; set; }
    }
}
