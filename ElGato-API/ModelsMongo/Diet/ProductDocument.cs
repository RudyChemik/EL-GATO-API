using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ElGato_API.ModelsMongo.Diet
{
    [BsonIgnoreExtraElements]
    public class ProductDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("code")]
        public string Code { get; set; }

        [BsonElement("product_name")]
        public string Product_name { get; set; }

        [BsonElement("nutrition_data_prepared_per")]
        public string? Nutrition_data_prepared_per { get; set; }

        [BsonElement("brands")]
        public string? Brands {  get; set; }

        [BsonElement("nutriments")]
        public NutrimentsDocument Nutriments { get; set; }


    }
}
