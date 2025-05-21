using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace Shared;

[BsonIgnoreExtraElements]
[BsonDiscriminator("element")]
[BsonKnownTypes(typeof(Quiz))]
public class Element
{
    [BsonId]
    [BsonIgnoreIfDefault]
    public ObjectId _id { get; set; }
    public int ElementId { get; set; }
    public string ElementNavn { get; set; }
    public string ElementBeskrivelse { get; set; }
    public string ElementType { get; set; } 
}