using MongoDB.Bson;


namespace Shared;

public class Maal
{
    public int Id { get; set; }
    public string MaalNavn { get; set; }
    public List<Delmaal> ListDelmaal { get; set; }
}