using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Shared;
[BsonIgnoreExtraElements]
public class Maal
{
    [BsonId]
    [BsonIgnoreIfDefault]
    public ObjectId _id { get; set; }
    public int MaalId { get; set; }
    public string MaalNavn { get; set; }
    public List<Delmaal> ListDelmaal { get; set; }
}