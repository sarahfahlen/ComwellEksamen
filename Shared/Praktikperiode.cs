using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Shared;

public class Praktikperiode
{
    [BsonId] // Brug denne som ID i stedet for _id
    [BsonRepresentation(BsonType.Int32)] 
    public int PraktikId { get; set; }
    public string PraktikNavn { get; set; }
    public int Skolevarighed { get; set; }
    public int Praktikvarighed { get; set; }
    public DateOnly StartDato { get; set; }
    public DateOnly SlutDato { get; set; }
    public Lokation Skole { get; set; }
    public List<Maal> ListMaal { get; set; }
}