using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Shared;
[BsonIgnoreExtraElements]
public class Kommentar
{
    [BsonId]
    [BsonIgnoreIfDefault]
    public ObjectId _id { get; set; }
    public int KommentarId { get; set; }
    public Bruger OprettetAf { get; set; }
    public DateOnly Dato { get; set; }
    public string Tekst { get; set; }
}