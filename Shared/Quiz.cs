using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace Shared;

[BsonIgnoreExtraElements]
[BsonDiscriminator("quiz")]
public class Quiz : Element
{
    public List<Spoergsmaal> ListSpoergsmaal { get; set; } = new();
}