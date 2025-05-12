using Shared;

namespace ComwellApp.Services.Elevplan;

public interface IElevplanService
{
    //Bruges til at oprette nye kommentarer på et delmål
    public Task TilfoejKommentar(Shared.Elevplan minPlan, int delmaalId, Kommentar nyKommentar);
    
    //Bruges til at redigere eksisterende kommentarer på et delmål
    public Task RedigerKommentar(Shared.Elevplan minPlan, int delmaalId, int kommentarId, string nyTekst);
//Bruges til at hente kommentar der passer til delmål og den rolle man er logget ind som
    public Kommentar? GetKommentar(Shared.Elevplan plan, int delmaalId, string brugerRolle);
    //Bruges til at hente alle elevplaner
    public List<Shared.Elevplan> GetAllElevplaner();
    //Bruges til at oprette en elevplan ud fra default - sendes til opret bruger
    public Task<Shared.Elevplan> OpretElevplan(Bruger ansvarlig);
    //Bruges til at oprette default skabelon til nye elever
    public Task<Shared.Elevplan> LavDefaultSkabelon(Bruger ansvarlig);
}