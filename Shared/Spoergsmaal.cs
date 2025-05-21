using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace Shared;

[BsonIgnoreExtraElements]
public class Spoergsmaal
{
    [BsonId]
    [BsonIgnoreIfDefault]
    public ObjectId _id { get; set; }
    public int SpoergsmaalId { get; set; }
    public string Tekst { get; set; }
    public List<Svarmulighed> Svar { get; set; } = new();
    public int KorrektIndex { get; set; }
}