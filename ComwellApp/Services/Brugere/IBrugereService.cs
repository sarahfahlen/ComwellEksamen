using Shared;

namespace ComwellApp.Services.Brugere;

public interface IBrugereService
{
    public Task TilfoejElev(Bruger nyBruger, Bruger ansvarlig, string skabelonType);
    Task<List<Bruger>> HentAlle();
    Task<Shared.Elevplan?> GetElevplanForUser(Bruger bruger);
    Task<List<Bruger>> HentAlleKøkkenchefer();
    Task<List<Lokation>> HentAlleLokationer();

}