
namespace Shared;


public class Underemne
{
    public int _id { get; set; }
    public string UnderemneNavn { get; set; }
    public string UnderemneBeskrivelse { get; set; }
    public List<Element> ListElement { get; set; } = new();
    
}