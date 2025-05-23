using Shared;
using ElevplanModel = Shared.Elevplan;

namespace ComwellApp.Services.Elevplan;

public interface IElevplanService
{
    //Bruges til at oprette nye kommentarer på et delmål
    public Task TilfoejKommentar(Shared.Elevplan minPlan, int delmaalId, Kommentar nyKommentar);

    //Bruges til at redigere eksisterende kommentarer på et delmål
    Task RedigerKommentar(Shared.Elevplan minPlan, int delmaalId, Kommentar redigeretKommentar);


    //Bruges til at hente kommentar der passer til delmål og den rolle man er logget ind som
    public Task<Kommentar?> GetKommentarAsync(int elevplanId, int delmaalId, string brugerRolle);

    //Bruges til at status opdateres for et delmål
    Task OpdaterStatus(Shared.Elevplan plan, Delmaal delmaal);

    //Bruges til at oprette default skabelon til nye elever
    Task<Shared.Elevplan> LavDefaultSkabelon(Bruger ansvarlig, string skabelonNavn, DateOnly startdato);
    //Bruges til at hente de filtrede mål på elevplanen
    Task<List<Maal>> HentFiltreredeMaal(int brugerId, int periodeIndex, string? valgtMaalNavn, string? valgtDelmaalType,
        string? soegeord, bool? filterStatus);

    Task TilfoejDelmaal(Shared.Elevplan plan, int maalId, Delmaal nytDelmaal);
    Task OpdaterDelmaal(Shared.Elevplan plan, int periodeIndex, int maalId, Delmaal opdateretDelmaal);
    Task SletDelmaal(Shared.Elevplan plan, int periodeIndex, int maalId, int delmaalId);
    
    Task<List<Maal>> HentMaalFraPeriode(int elevplanId, int periodeIndex);
    Task<List<string>> HentDelmaalTyperFraPeriode(int elevplanId, int periodeIndex);

    Task<List<Delmaal>> HentKommendeDeadlines(int brugerId);
    
    Task OpdaterIgang(Shared.Elevplan plan, Delmaal delmaal);
}