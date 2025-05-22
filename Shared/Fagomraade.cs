

namespace Shared;


public class Fagomraade
{
    public int Id { get; set; }
    public string FagomraadeNavn { get; set; }
    public string FagomraadeBeskrivelse { get; set; }

    public List<Underemne> ListUnderemne { get; set; } = new();
}

