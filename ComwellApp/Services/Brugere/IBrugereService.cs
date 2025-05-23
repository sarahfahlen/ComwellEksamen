using Shared;

namespace ComwellApp.Services.Brugere;

public interface IBrugereService
{
    Task TilfoejElev(Bruger nyBruger, Bruger ansvarlig, string skabelonType);

    Task ArkiverElev(Bruger elev);
    
    Task<List<Bruger>> HentAlle();
    Task<List<Bruger>> HentAlleElever();
    Task<Shared.Elevplan?> HentElevplanForBruger(int brugerId, int forespoergerId);
    Task<List<Bruger>> HentKoekkencheferForLokation(int lokationId);
    Task<List<string>> HentAlleErhverv();
    Task<List<string>> HentAlleKurser();
    Task OpdaterBruger(Bruger bruger);

    Task<List<Bruger>> HentFiltreredeElever(string soegeord, string kursus, string erhverv,
        int? deadline, string rolle, string? status, int? afdelingId);

    Task<byte[]> EksporterFiltreredeElever(
        string soegeord,
        string kursus,
        string erhverv,
        int? deadline,
        string rolle,
        string? status,
        int? afdelingId);
}