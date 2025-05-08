using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Shared;

public class Elevplan
{
    [BsonId] // Brug denne som ID i stedet for _id
    [BsonRepresentation(BsonType.Int32)] 
    public int ElevplanId { get; set; }
    public Bruger Ansvarlig { get; set; }
    public List<Praktikperiode> ListPerioder { get; set; }
}