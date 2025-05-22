namespace Shared;

//ignorerer de elementer der mangler i MongoDB, bruges til vores skabelon

public class Elevplan
{
    public int _id { get; set; }
    public DateOnly? ElevStartDato { get; set; }
    public Bruger Ansvarlig { get; set; }
    
    public string? SkabelonNavn { get; set; }
    public List<Praktikperiode> ListPerioder { get; set; }
}