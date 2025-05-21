using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Shared;
[BsonIgnoreExtraElements]
public class Delmaal
{
    [BsonId]
    [BsonIgnoreIfDefault]
    public ObjectId _id { get; set; }
    public int DelmaalId { get; set; }
    [Required(ErrorMessage = "Delmålstype er påkrævet")]
    public string DelmaalType { get; set; }
    [Required(ErrorMessage = "Titel er påkrævet")]
    public string Titel { get; set; }
    public string? Beskrivelse {get;set;}
    public string? Ansvarlig { get; set; }
    public DateOnly? Deadline { get; set; }
    public string? DeadlineKommentar { get; set; }
    
    public int? DageTilDeadline { get; set; }
    public bool Status { get; set; } = false;
    
    public bool Igang {get; set;} = false;
    
    public string? StatusLog { get; set; }
    
    public List<Kommentar> Kommentarer { get; set; } = new ();
    
    public List <Opgaver> ListOpgaver { get; set; } = new ();
}