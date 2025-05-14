using System.Net.Http.Json;
using Shared;
namespace ComwellApp.Services.Brugere;

public class BrugereServiceServer : IBrugereService
{
    private readonly HttpClient http;
    private readonly IdGeneratorService _idGenerator;

    public BrugereServiceServer(HttpClient http, IdGeneratorService idGenerator)
    {
        this.http = http;
        _idGenerator = idGenerator;
    }

    private List<Shared.Elevplan> _allePlaner = new();
    private List<Bruger> _alleBrugere = new();
    
   public async Task TilfoejElev(Bruger nyBruger, Bruger ansvarlig, string skabelonType)
{
    //Henter alle eksisterende brugere og opretter nyt BrugerId ud fra denne liste
    var alleBrugere = await HentAlle();
    nyBruger.BrugerId = _idGenerator.GenererNytId(alleBrugere, b => b.BrugerId);

    //Hent skabelon fra vores elevplan controller
    var response = await http.GetAsync($"api/elevplan/skabelon/{skabelonType}");
    if (!response.IsSuccessStatusCode)
    {
        var fejl = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"[TilfoejElev] FEJL: {fejl}");
        throw new Exception($"Fejl ved hentning af skabelon '{skabelonType}'");
    }

    var plan = await response.Content.ReadFromJsonAsync<Shared.Elevplan>();
    if (plan == null)
        throw new Exception("Skabelonen kunne ikke konverteres til Elevplan");

    //Sætter elevplanId til at matche brugerId, og tildeler den ansvarlige baseret på hvad der kommer fra frontend
    plan.ElevplanId = nyBruger.BrugerId;
    plan.Ansvarlig = ansvarlig;

    //Sørger for at generere ID til alle mål, delmål og opgaver i elevplanen samt sætte status til false
    var alleMaal = new List<Maal>();
    var alleDelmaal = new List<Delmaal>();
    var alleOpgaver = new List<Opgaver>();

    foreach (var periode in plan.ListPerioder)
    {
        foreach (var maal in periode.ListMaal)
        {
            maal.MaalId = _idGenerator.GenererNytId(alleMaal, m => m.MaalId);
            alleMaal.Add(maal);

            foreach (var delmaal in maal.ListDelmaal)
            {
                delmaal.DelmaalId = _idGenerator.GenererNytId(alleDelmaal, d => d.DelmaalId);
                delmaal.Status = false;
                alleDelmaal.Add(delmaal);

                foreach (var opgave in delmaal.ListOpgaver)
                {
                    opgave.OpgaveId = _idGenerator.GenererNytId(alleOpgaver, o => o.OpgaveId);
                    opgave.OpgaveGennemfoert = false;
                    alleOpgaver.Add(opgave);
                }
            }
        }
    }

    //Tildeler planen til den nye elev
    nyBruger.MinElevplan = plan;

    //Kalder bruger controller og gemmer eleven
    var postResponse = await http.PostAsJsonAsync("api/brugere", nyBruger);
    if (!postResponse.IsSuccessStatusCode)
    {
        var fejl = await postResponse.Content.ReadAsStringAsync();
        Console.WriteLine($"[TilfoejElev] FEJL ved oprettelse: {fejl}");
        throw new Exception("Kunne ikke oprette bruger med elevplan");
    }
}


    public async Task<List<Bruger>> HentAlle()
    {
        return await http.GetFromJsonAsync<List<Bruger>>("api/brugere/elever")
               ?? new List<Bruger>();
    }

    public async Task<List<Bruger>> HentAlleKøkkenchefer()
    {
        var kokke = await http.GetFromJsonAsync<List<Bruger>>("api/brugere/køkkenchefer");
        return kokke ?? new List<Bruger>();
    }
    public async Task<List<Lokation>> HentAlleLokationer()
    {
        var result = await http.GetFromJsonAsync<List<Lokation>>("api/brugere/lokationer");
        return result ?? new List<Lokation>();
    }


    public Task<Shared.Elevplan?> GetElevplanForUser(Bruger bruger)
    {
        throw new NotImplementedException();
    }
}