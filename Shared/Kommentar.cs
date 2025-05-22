namespace Shared;

public class Kommentar
{
    public int _id { get; set; }
    public string OprettetAfNavn { get; set; }
    
    public string OprettetAfRolle { get; set; }
    public DateOnly Dato { get; set; }
    public string Tekst { get; set; }
    
    public string? KommentarBillede { get; set; }
    
}