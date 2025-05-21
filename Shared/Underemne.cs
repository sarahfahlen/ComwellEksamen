using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace Shared;

[BsonIgnoreExtraElements]
public class Underemne
{
    [BsonId]
    [BsonIgnoreIfDefault]
    public ObjectId _id { get; set; }
    public int UnderemneId { get; set; }
    public string UnderemneNavn { get; set; }
    public string UnderemneBeskrivelse { get; set; }
    public List<Element> ListElement { get; set; } = new();
    
}