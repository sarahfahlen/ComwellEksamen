using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Shared;

public class Kommentar
{
    [BsonId] // Brug denne som ID i stedet for _id
    [BsonRepresentation(BsonType.Int32)] 
    public int KommentarId { get; set; }
    public Bruger OprettetAf { get; set; }
    public DateOnly Dato { get; set; }
    public string Tekst { get; set; }
}