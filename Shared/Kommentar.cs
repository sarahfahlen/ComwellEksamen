namespace Shared;

public class Kommentar
{
    public int Id { get; set; }
    public string OprettetAfNavn { get; set; }
    
    public string OprettetAfRolle { get; set; }
    public DateOnly Dato { get; set; }
    public string Tekst { get; set; }
    
    public string? KommentarBillede { get; set; }
    
}