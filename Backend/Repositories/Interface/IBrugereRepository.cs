using Shared; 
namespace Backend.Repositories.Interface;

public interface IBrugereRepository
{
    Task TilfoejElev(Bruger nyBruger);
    
    Task ArkiverElev(Bruger elev);
    
    Task<List<Bruger>> HentAlle();
    Task<List<Bruger>> HentAlleElever();

    Task<List<Bruger>> HentAlleKøkkenchefer();
    Task<List<string>> HentErhverv();
    Task<List<string>> HentKurser();
    Task<Elevplan?> HentElevplanForBruger(int brugerId, int forespoergerId);
    Task OpdaterBruger(Bruger bruger);
    //Bliver brugt til at opdatere skolelokation(id) til en elev på pågældene periode
    Task OpdaterSkoleId(int brugerId, int periodeIndex, int? nySkoleId);
    Task<List<Bruger>> HentFiltreredeElever(
        string soegeord,
        string kursus,
        string erhverv,
        int? deadline,
        string rolle,
        string status,
        int? afdelingId);
    // I IBrugereRepository.cs
    Task OpdaterBillede(int brugerId, string sti);

}