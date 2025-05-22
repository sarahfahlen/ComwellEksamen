using MongoDB.Bson;


namespace Shared;

public class Maal
{
    public int _id { get; set; }
    public string MaalNavn { get; set; }
    public List<Delmaal> ListDelmaal { get; set; }
}