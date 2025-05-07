using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace Shared;

public class Bruger
{
    [BsonId] // Brug denne som ID i stedet for _id
    [BsonRepresentation(BsonType.Int32)] 
    public int BrugerId { get; set; }
    public string Navn { get; set; }
    public string Email { get; set; }
    public string Adgangskode { get; set; }
    public int Telefon { get; set; }
    public string Rolle { get; set; }
    public string Billede { get; set; }
    public DateOnly StartDato { get; set; }
    public Lokation Koekken { get; set; }
}