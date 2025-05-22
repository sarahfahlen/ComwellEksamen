namespace Shared;

public class Praktikperiode
{
    public int Id { get; set; }
    public string PraktikNavn { get; set; }
    public int? Skolevarighed { get; set; }
    public int Praktikvarighed { get; set; }
    public DateOnly? StartDato { get; set; }
    public DateOnly? SlutDato { get; set; }
    public Lokation? Skole { get; set; }
    public List<Maal> ListMaal { get; set; }
}