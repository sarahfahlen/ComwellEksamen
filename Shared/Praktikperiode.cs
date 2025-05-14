using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Shared;
[BsonIgnoreExtraElements]
public class Praktikperiode
{
    [BsonId]
    [BsonIgnoreIfDefault]
    public ObjectId _id { get; set; }
    public int PraktikId { get; set; }
    public string PraktikNavn { get; set; }
    public int Skolevarighed { get; set; }
    public int Praktikvarighed { get; set; }
    public DateOnly StartDato { get; set; }
    public DateOnly SlutDato { get; set; }
    public Lokation Skole { get; set; }
    public List<Maal> ListMaal { get; set; }
}