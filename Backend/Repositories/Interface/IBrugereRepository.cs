using Shared; 
namespace Backend.Repositories.Interface;

public interface IBrugereRepository
{
    Task TilfoejElev(Bruger nyBruger);
    Task<List<Bruger>> HentAlle();
    Task<List<Bruger>> HentAlleKøkkenchefer();
    Task<List<Lokation>> HentAlleLokationer();

}