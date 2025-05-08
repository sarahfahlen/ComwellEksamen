using ComwellApp.Services.Elevplan;
using Shared;
namespace ComwellApp.Services.Brugere;


public class BrugereServiceMock : IBrugereService
{
    private readonly ElevplanServiceMock _elevplanService;
    private readonly IdGeneratorService _idGenerator;

    public BrugereServiceMock(ElevplanServiceMock elevplanService, IdGeneratorService idGenerator)
    {
        _elevplanService = elevplanService;
        _idGenerator = idGenerator;
    }
    
    public static List<Bruger> brugere = new List<Bruger> { Kasper, Emil, Frank };

    public async Task TilfoejElev(Bruger nyBruger, Bruger ansvarlig)
    {
        nyBruger.BrugerId = _idGenerator.GenererNytId(brugere, b => b.BrugerId);
        nyBruger.MinElevplan = await _elevplanService.OpretElevplan(ansvarlig);
        brugere.Add(nyBruger);
    }
    
    
    public static Bruger Kasper = new Bruger
    {
        BrugerId = 1, Navn = "Kasper", Adgangskode = "1234", Email = "kasper@mail.com", BrugerTelefon = 76546789,
        Rolle = "Køkkenchef",
    };
    public static Bruger Emil = new Bruger
    {
        BrugerId = 2, Navn = "Emil", Adgangskode = "1234", Email = "emil@mail.com", BrugerTelefon = 87907652,
        Rolle = "Elev"
    };
    public static Bruger Frank = new Bruger
    {
        BrugerId = 3, Navn = "Frank", Adgangskode = "1234", Email = "frank@mail.com", BrugerTelefon = 64572358,
        Rolle = "FaglærtKok"
    };
}