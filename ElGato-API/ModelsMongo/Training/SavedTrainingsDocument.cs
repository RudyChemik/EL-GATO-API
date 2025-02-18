using MongoDB.Bson;

namespace ElGato_API.ModelsMongo.Training
{
    public class SavedTrainingsDocument
    {
        public ObjectId Id { get; set; }
        public string UserId { get; set; }
        public List<SavedTrainings> SavedTrainings { get; set; } = new List<SavedTrainings>();
    }

    public class SavedTrainings
    {
        public string Name { get; set; }
        public int PublicId { get; set; }
        public List<SavedExercises> Exercises { get; set; } = new List<SavedExercises>();
    }

    public class SavedExercises
    {
        public string Name { get; set; }
        public int PublicId { get; set; }
    }

}
