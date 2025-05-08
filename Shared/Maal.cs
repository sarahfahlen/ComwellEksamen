using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Shared;

public class Maal
{
    [BsonId] // Brug denne som ID i stedet for _id
    [BsonRepresentation(BsonType.Int32)] 
    public int MaalId { get; set; }
    public string MaalNavn { get; set; }
    public List<Delmaal> ListDelmaal { get; set; }
}