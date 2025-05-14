using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace Shared;
[BsonIgnoreExtraElements]
public class Opgaver
{
    [BsonId]
    [BsonIgnoreIfDefault]
    public ObjectId _id { get; set; }
    public int OpgaveId { get; set; } 
    public string OpgaveNavn { get; set; }
    public bool OpgaveGennemfoert { get; set; }
    public string OpgaveDetaljer { get; set; }
}