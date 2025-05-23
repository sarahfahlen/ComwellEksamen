using ComwellApp.Services;
using ComwellApp.Services.Brugere;
using ComwellApp.Services.Elevplan;
using Shared;
using NotImplementedException = System.NotImplementedException;

public class BrugereServiceMock : IBrugereService
{
    private readonly ElevplanServiceMock _elevplanService;
    private readonly IdGeneratorService _idGenerator;
    private static readonly List<Bruger> _brugere = new();
    public BrugereServiceMock(ElevplanServiceMock elevplanService)
    {
        _elevplanService = elevplanService;
    }

    public BrugereServiceMock(ElevplanServiceMock elevplanService, IdGeneratorService idGenerator)
    {
        _elevplanService = elevplanService;
        _idGenerator = idGenerator;

        _brugere.AddRange(new[]
        {
            new Bruger
            {
                _id = 1,
                Navn = "Kasper",
                Adgangskode = "1234",
                Email = "kasper@mail.com",
                BrugerTelefon = "76546789",
                Rolle = "Køkkenchef"
            },
            new Bruger
            {
                _id = 2,
                Navn = "Frank",
                Adgangskode = "1234",
                Email = "frank@mail.com",
                BrugerTelefon = "64572358",
                Rolle = "FaglærtKok"
            },
            new Bruger
            {
                _id = 3,
                Navn = "Jane",
                Adgangskode = "1234",
                Email = "jane@mail.com",
                BrugerTelefon = "76546489",
                Rolle = "HR"
            },
            new Bruger
            {
                _id = 4,
                Navn = "Ole",
                Adgangskode = "1234",
                Email = "Ole@mail.com",
                BrugerTelefon = "71542789",
                Rolle = "Køkkenchef"
            },
        });

        var emil = new Bruger
        {
            _id = _idGenerator.GenererNytId(_brugere, b => b._id),
            Navn = "Emil",
            Adgangskode = "1234",
            Email = "emil@mail.com",
            BrugerTelefon = "87907652",
            Rolle = "Elev",
            MinElevplan = null
        };
 
        _brugere.Add(emil);
    }

    public Task ArkiverElev(Bruger elev)
    {
        throw new NotImplementedException();
    }

    public Task<List<Bruger>> HentAlle()
    {
        return Task.FromResult(_brugere);
    }

    public Task<List<Bruger>> HentAlleElever()
    {
        throw new NotImplementedException();
    }

    public Task<Elevplan?> HentElevplanForBruger(int brugerId, int forespoergerId)
    {
        throw new NotImplementedException();
    }

    public Task<Elevplan?> HentElevplanForBruger(int brugerId)
    {
        throw new NotImplementedException();
    }

    public Task<List<Bruger>> HentKoekkencheferForLokation(int lokationId)
    {
        throw new NotImplementedException();
    }
    
    public Task<List<Lokation>> HentAlleLokationer()
    {
        throw new NotImplementedException();
    }

    public Task<List<string>> HentAlleErhverv()
    {
        throw new NotImplementedException();
    }

    public Task<List<string>> HentAlleKurser()
    {
        throw new NotImplementedException();
    }

    public Task OpdaterBruger(Bruger bruger)
    {
        throw new NotImplementedException();
    }

    public Task OpdaterSkoleId(int brugerId, int periodeIndex, int? nySkoleId)
    {
        throw new NotImplementedException();
    }

    public async Task<List<Bruger>> HentFiltreredeElever(string soegeord, string kursus, string erhverv, int? deadline, string rolle, string? status, int? afdelingId, bool? aktiv)
    {
        throw new NotImplementedException();
    }

public Task<byte[]> EksporterFiltreredeElever(
    string soegeord,
    string kursus,
    string erhverv,
    int? deadline,
    string rolle,
    string? status,
    int? afdelingId,
    bool? aktiv) 
    {
        throw new NotImplementedException();
    }

    public async Task TilfoejElev(Bruger nyBruger, Bruger ansvarlig, string skabelonType)
    {
        nyBruger._id = _idGenerator.GenererNytId(_brugere, b => b._id);
        nyBruger.MinElevplan = await _elevplanService.OpretElevplan(ansvarlig, skabelonType);
        _brugere.Add(nyBruger);
        Console.WriteLine("Brugere i systemet:");
        foreach (var bruger in _brugere)
        {
            Console.WriteLine($"Navn: {bruger.Navn}, Email: {bruger.Email}, Adgangskode: {bruger.Adgangskode}");
        }
    }
}