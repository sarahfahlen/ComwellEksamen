using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace Shared;

[BsonIgnoreExtraElements]
public class Svarmulighed
{
    [BsonId]
    [BsonIgnoreIfDefault]
    public ObjectId _id { get; set; }
    public int SvarmulighedId { get; set; }
    public string Tekst { get; set; }
}