using ElGato_API.Models.User;
using System.ComponentModel.DataAnnotations;

namespace ElGato_API.Models.Requests
{
    public class ReportedIngredients
    {
        [Key]
        public int Id { get; set; }
        public string IngredientId { get; set; }
        public string IngredientName { get; set; }
        public string UserId { get; set; }
        public int Cause { get; set; }
        public bool Resolved { get; set; } = false;
        public string? ResolvedById { get; set; }

    }
}
