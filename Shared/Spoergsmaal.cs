namespace Shared;

public class Spoergsmaal
{
    public int Id { get; set; }
    public string Tekst { get; set; }
    public List<Svarmulighed> Svar { get; set; } = new();
    public int KorrektIndex { get; set; }
}