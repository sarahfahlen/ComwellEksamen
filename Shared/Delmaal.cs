using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Shared;

public class Delmaal
{
    [BsonId] // Brug denne som ID i stedet for _id
    [BsonRepresentation(BsonType.Int32)] 
    public int DelmaalID { get; set; }
    public string DelmaalType { get; set; }
    public string Beskrivelse { get; set; }
    public string Ansvarlig { get; set; }
    public DateOnly? Deadline { get; set; }
    public string DeadlineKommentar { get; set; }
    public bool Status { get; set; }
}