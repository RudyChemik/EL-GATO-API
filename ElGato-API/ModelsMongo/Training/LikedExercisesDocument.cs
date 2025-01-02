using MongoDB.Bson;

namespace ElGato_API.ModelsMongo.Training
{
    public class LikedExercisesDocument
    {
        public ObjectId Id { get; set; }
        public string UserId { get; set; }
        public List<LikedExercise> Premade { get; set; } = new List<LikedExercise>();
        public List<string> Own { get; set; } = new List<string>();
    }

    public class LikedExercise
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
