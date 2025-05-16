using Shared; 
namespace Backend.Repositories.Interface;

public interface IBrugereRepository
{
    Task TilfoejElev(Bruger nyBruger);
    Task<List<Bruger>> HentAlle();
    Task<List<Bruger>> HentAlleElever();

    Task<List<Bruger>> HentAlleKøkkenchefer();
    Task<List<Lokation>> HentAlleLokationer();
    Task<List<string>> HentErhverv();
    Task<Elevplan?> HentElevplanForBruger(int brugerId);
    Task<List<Bruger>> HentFiltreredeElever(string soegeord, string lokation, string kursus, string erhverv, int? deadline, string rolle, string? brugerLokation);

}