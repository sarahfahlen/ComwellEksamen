using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Shared;

//ignorerer de elementer der mangler i MongoDB, bruges til vores skabelon
[BsonIgnoreExtraElements]
public class Elevplan
{
    [BsonId]  
    [BsonIgnoreIfDefault]
    public ObjectId _id { get; set; }
    public int ElevplanId { get; set; }
    public Bruger Ansvarlig { get; set; }
    public List<Praktikperiode> ListPerioder { get; set; }
}