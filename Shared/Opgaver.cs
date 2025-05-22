
namespace Shared;

public class Opgaver
{
 
    public int _id { get; set; } 
    public string OpgaveNavn { get; set; }
    public bool OpgaveGennemfoert { get; set; } = false;
    public string? OpgaveDetaljer { get; set; }
    
    public string? StatusLogOpgave { get; set; }
}