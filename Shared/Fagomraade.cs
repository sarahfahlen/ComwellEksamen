using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Shared;
[BsonIgnoreExtraElements]

public class Fagomraade
{
    [BsonId]
    [BsonIgnoreIfDefault]
    public ObjectId _id { get; set; }
    public int FagomraadeId { get; set; }
    public string FagomraadeNavn { get; set; }
    public string FagomraadeBeskrivelse { get; set; }

    public List<Underemne> ListUnderemne { get; set; } = new();
}

