using System.ComponentModel.DataAnnotations;

namespace ElGato_API.Models.Feed
{
    public class Creator
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Pfp { get; set; }
        public List<Challange> Challenges { get; set; } = new List<Challange>();
    }
}
