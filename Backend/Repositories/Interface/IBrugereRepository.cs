using Shared; 
namespace Backend.Repositories.Interface;

public interface IBrugereRepository
{
    Task TilfoejElev(Bruger nyBruger);
    Task<List<Bruger>> HentAlle();
    Task<List<Bruger>> HentAlleElever();

    Task<List<Bruger>> HentAlleKøkkenchefer();
    Task<List<Lokation>> HentAlleLokationer();
    Task<Elevplan?> HentElevplanForBruger(int brugerId);
    Task<List<Bruger>> HentFiltreredeElever(string? navn, string? lokation, string? kursus, string? erhverv, int? deadlineDage);

}