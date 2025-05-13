using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace Shared;

public class Opgaver
{
    [BsonId] // Brug denne som ID i stedet for _id
    [BsonRepresentation(BsonType.Int32)] 
    public int OpgaveID { get; set; } 
    public string OpgaveNavn { get; set; }
    public bool OpgaveGennemfoert { get; set; }
    public string OpgaveDetaljer { get; set; }
}