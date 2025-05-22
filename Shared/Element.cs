
namespace Shared;


public class Element
{
    public int Id { get; set; }
    public int ElementId { get; set; }
    public string ElementNavn { get; set; }
    public string ElementBeskrivelse { get; set; }
    public string ElementType { get; set; } 
    public List<Spoergsmaal> ListSpoergsmaal { get; set; } = new();
}