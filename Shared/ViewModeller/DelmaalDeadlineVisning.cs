namespace Shared.ViewModeller;

public class DelmaalDeadlineVisning
{

    public string ElevNavn { get; set; }  
    public string Lokation { get; set; } 
    public string Erhverv { get; set; } 
    public string DelmaalTitel { get; set; } 
    public DateOnly? Deadline { get; set; } 
    public bool ErOverskredet { get; set; }
    public int? DageOverskredet { get; set; } 
    public int? AntalDageTilDeadline { get; set; }
}