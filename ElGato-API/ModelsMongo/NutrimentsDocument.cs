using MongoDB.Bson.Serialization.Attributes;

namespace ElGato_API.ModelsMongo
{
    [BsonIgnoreExtraElements]
    public class NutrimentsDocument
    {
        [BsonElement("fat")]
        public double Fat { get; set; }

        [BsonElement("proteins")]
        public double Proteins { get; set; }

        [BsonElement("carbohydrates")]
        public double Carbs { get; set; }

        [BsonElement("energy-kcal_100g")]
        public double EnergyKcal { get; set; }

    }
}
