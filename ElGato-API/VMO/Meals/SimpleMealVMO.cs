using MongoDB.Bson;

namespace ElGato_API.VMO.Meals
{
    public class SimpleMealVMO
    {
        public ObjectId Id { get; set; }
        public string Name { get; set; }
        public string Time { get; set; }
        public string Img { get; set; }
        public int SavedCounter { get; set; } = 0;
        public int LikedCounter { get; set; } = 0;
        public string CreatorName { get; set; }
        public string? CreatorPfp { get; set; }
        public bool Liked { get; set; } = false;
        public bool Saved { get; set; } = false;
    }
}
