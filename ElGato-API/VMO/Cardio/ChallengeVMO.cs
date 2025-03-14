using ElGato_API.Models.Feed;
using System.Text.Json.Serialization;

namespace ElGato_API.VMO.Cardio
{
    public class ChallengeVMO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Badge { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ChallangeType Type { get; set; } = ChallangeType.None;
        public int MaxTimeMinutes { get; set; } = 0;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ChallengeGoalType GoalType { get; set; } = ChallengeGoalType.None;
        public double GoalValue { get; set; } = 0;
        public DateTime? EndDate { get; set; }
        public CreatorVMO? Creator { get; set; }
    }
}
