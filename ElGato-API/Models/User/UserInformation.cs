namespace ElGato_API.Models.User
{
    public class UserInformation
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public AppUser AppUser { get; set; }
        public double? Age { get; set; }
        public double? Height { get; set; }
        public double? Weight { get; set; }
        public string? Country { get; set; }
        public int? TrainingDays { get; set; }
        public int? Goal {  get; set; }
        public int? Sleep { get; set; }
        public int? BodyType { get; set; }
        public int? JobType { get; set; }
        public int? DailyTimeSpendWorking { get; set; }
        public bool? Woman { get; set; }
    }
  
}
