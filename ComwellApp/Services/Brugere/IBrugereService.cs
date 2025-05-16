using Shared;

namespace ComwellApp.Services.Brugere;

public interface IBrugereService
{
    public Task TilfoejElev(Bruger nyBruger, Bruger ansvarlig, string skabelonType);
    Task<List<Bruger>> HentAlle();
    Task<Shared.Elevplan?> HentElevplanForBruger(int brugerId);
    Task<List<Bruger>> HentKoekkencheferForLokation(int lokationId);

    Task<List<Lokation>> HentAlleLokationer();
    Task<List<Bruger>> HentFiltreredeElever(string? navn, string? lokation, string? kursus, string? erhverv, int? deadlineDage);

}