using ComwellApp.Services;
using ComwellApp.Services.Brugere;
using ComwellApp.Services.Elevplan;
using Shared;

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
                BrugerId = 1,
                Navn = "Kasper",
                Adgangskode = "1234",
                Email = "kasper@mail.com",
                BrugerTelefon = 76546789,
                Rolle = "Køkkenchef"
            },
            new Bruger
            {
                BrugerId = 2,
                Navn = "Frank",
                Adgangskode = "1234",
                Email = "frank@mail.com",
                BrugerTelefon = 64572358,
                Rolle = "FaglærtKok"
            }
        });

        var emil = new Bruger
        {
            BrugerId = _idGenerator.GenererNytId(_brugere, b => b.BrugerId),
            Navn = "Emil",
            Adgangskode = "1234",
            Email = "emil@mail.com",
            BrugerTelefon = 87907652,
            Rolle = "Elev",
            MinElevplan = null
        };
 
        _brugere.Add(emil);
    }

    public Task<List<Bruger>> HentAlle()
    {
        return Task.FromResult(_brugere);
    }

    public Task<Elevplan?> GetElevplanForUser(Bruger bruger)
    {
        var elevplan = _elevplanService.GetAllElevplaner()
            .FirstOrDefault(p => p.Ansvarlig?.BrugerId == bruger.BrugerId);
        return Task.FromResult(elevplan);
    }


    public async Task TilfoejElev(Bruger nyBruger, Bruger ansvarlig)
    {
        nyBruger.BrugerId = _idGenerator.GenererNytId(_brugere, b => b.BrugerId);
        nyBruger.MinElevplan = await _elevplanService.OpretElevplan(ansvarlig);
        _brugere.Add(nyBruger);
        Console.WriteLine("Brugere i systemet:");
        foreach (var bruger in _brugere)
        {
            Console.WriteLine($"Navn: {bruger.Navn}, Email: {bruger.Email}, Adgangskode: {bruger.Adgangskode}");
        }
    }
}