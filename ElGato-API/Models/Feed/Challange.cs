using ElGato_API.Models.Feed;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElGato_API.Models.Feed
{
    public class Challange
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Badge { get; set; }
        public ChallangeType Type { get; set; } = ChallangeType.None;
        public int MaxTimeMinutes { get; set; } = 0;
        public ChallengeGoalType GoalType { get; set; } = ChallengeGoalType.None;
        public double GoalValue { get; set; } = 0;
        public DateTime? EndDate { get; set; }

        [ForeignKey("CreatorId")]
        public int? CreatorId { get; set; }
        public Creator? Creator { get; set; }
    }

    public enum ChallangeType
    {
        None,
        Running,
        Swimming,
        Walking,
        CaloriesBurnt,
        Bike,
        March,
        Activity,
    }

    public enum ChallengeGoalType
    {
        None,
        TotalDistanceKm,
        TotalCalories,
        TotalElevation,
        TotalActivities
    }
}
