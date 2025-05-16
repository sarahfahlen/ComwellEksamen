using Shared;

namespace ComwellApp.Services.Brugere;

public interface IBrugereService
{
    public Task TilfoejElev(Bruger nyBruger, Bruger ansvarlig, string skabelonType);
    Task<List<Bruger>> HentAlle();
    Task<Shared.Elevplan?> HentElevplanForBruger(int brugerId);
    Task<List<Bruger>> HentKoekkencheferForLokation(int lokationId);

    Task<List<Lokation>> HentAlleLokationer();
    Task<List<string>> HentAlleErhverv();

    Task<List<Bruger>> HentFiltreredeElever(string soegeord, string lokation, string kursus, string erhverv, int? deadline, string rolle, string? brugerLokation);

}