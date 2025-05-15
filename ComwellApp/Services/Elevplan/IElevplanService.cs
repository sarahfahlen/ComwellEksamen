using Shared;
using ElevplanModel = Shared.Elevplan;

namespace ComwellApp.Services.Elevplan;

public interface IElevplanService
{
    //Bruges til at oprette nye kommentarer på et delmål
    public Task TilfoejKommentar(Shared.Elevplan minPlan, int delmaalId, Kommentar nyKommentar);
    
    //Bruges til at redigere eksisterende kommentarer på et delmål
    public Task RedigerKommentar(Shared.Elevplan minPlan, int delmaalId, int kommentarId, string nyTekst);
    //Bruges til at hente kommentar der passer til delmål og den rolle man er logget ind som
    public Task<Kommentar?> GetKommentarAsync(int elevplanId, int delmaalId, string brugerRolle);
    //Bruges til at oprette default skabelon til nye elever
    public Task<Shared.Elevplan> LavDefaultSkabelon(Bruger ansvarlig, string skabelonNavn);
    //Bruges til at hente de filtrede mål på elevplanen
    List<Maal> HentFiltreredeMaal(Shared.Elevplan plan, int periodeIndex, string? valgtMaalNavn, string? valgtDelmaalType, string? søgeord, bool? filterStatus);


}