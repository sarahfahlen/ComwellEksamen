

namespace Shared;


public class Fagomraade
{
    public int _id { get; set; }
    public string FagomraadeNavn { get; set; }
    public string FagomraadeBeskrivelse { get; set; }

    public List<Underemne> ListUnderemne { get; set; } = new();
}

