using System.Net.Http.Json;
using ComwellApp.Services.Elevplan;
using Shared;

namespace ComwellApp.Services.Brugere;

// Dette er implementation af IBrugereService, som sender data frem og tilbage med backend.
public class BrugereServiceServer : IBrugereService
{
    private readonly HttpClient http; // Bruges til at sende HTTP-requests til vores backend
    private readonly IdGeneratorService _idGenerator; // Bruges til at generere unikke ID’er til fx elevplaner, delmål og elever
    private readonly IElevplanService _elevplanService;
    private static readonly List<Bruger> _brugere = new();
    public BrugereServiceServer(HttpClient http, IdGeneratorService idGenerator, IElevplanService elevplanService)
    {
        this.http = http;
        _idGenerator = idGenerator;
        _elevplanService = elevplanService;
    }
    
    
    // Interne lister (bliver dog ikke rigtig brugt her)
    private List<Shared.Elevplan> _allePlaner = new();
    private List<Bruger> _alleBrugere = new();

   
    // Oprettelse af ny elev + automatisk elevplan
   
    public async Task TilfoejElev(Bruger nyBruger, Bruger ansvarlig, string skabelonType)
    {
        // Først henter vi ALLE brugere, for at kunne generere et unikt BrugerId
        var alleBrugere = await HentAlle();
        nyBruger._id = _idGenerator.GenererNytId(alleBrugere, b => b._id);

        // Henter elevplan-skabelon fra backend (fx "KokSkabelon")
        var plan = await _elevplanService.LavDefaultSkabelon(ansvarlig, skabelonType, nyBruger.StartDato!.Value);


        // Gør planen klar: tildel ansvarlig, elevplanId og beregner deadlines dynamisk
        plan._id = nyBruger._id;
        plan.Ansvarlig = ansvarlig;
        
        BeregnDeadlinesIElevplan(plan);

        //  Gennemgår ALLE mål, delmål og opgaver og giver dem unikke ID'er og sætter status til "ikke gennemført"
        var alleMaal = new List<Maal>();
        var alleDelmaal = new List<Delmaal>();
        var alleOpgaver = new List<Opgaver>();

        foreach (var periode in plan.ListPerioder)
        {
            foreach (var maal in periode.ListMaal)
            {
                maal._id = _idGenerator.GenererNytId(alleMaal, m => m._id);
                alleMaal.Add(maal);

                foreach (var delmaal in maal.ListDelmaal)
                {
                    delmaal._id = _idGenerator.GenererNytId(alleDelmaal, d => d._id);
                    delmaal.Status = false;
                    alleDelmaal.Add(delmaal);

                    foreach (var opgave in delmaal.ListOpgaver)
                    {
                        opgave._id = _idGenerator.GenererNytId(alleOpgaver, o => o._id);
                        opgave.OpgaveGennemfoert = false;
                        alleOpgaver.Add(opgave);
                    }
                }
            }
        }

        // Elevplanen færdiggøres og tilknyttes brugeren
        nyBruger.MinElevplan = plan;

        // Til sidst gemmer vi den nye bruger (inkl. elevplan) i databasen
        var postResponse = await http.PostAsJsonAsync("api/brugere", nyBruger);
        if (!postResponse.IsSuccessStatusCode)
        {
            var fejl = await postResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"[TilfoejElev] FEJL ved oprettelse: {fejl}");
            throw new Exception("Kunne ikke oprette bruger med elevplan");
        }
    }
    
    public async Task ArkiverElev(Bruger elev)
    {
        elev.Aktiv = false; // her sætter vi aktiv til false - det betyder arkiveret

        var response = await http.PutAsJsonAsync($"api/brugere/arkiver", elev);

        if (!response.IsSuccessStatusCode)
        {
            var fejl = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[ArkiverElev] FEJL: {fejl}");
            throw new Exception("Kunne ikke arkivere elev");
        }
    }


   
    
    //Henter alle brugere fra backend
    public async Task<List<Bruger>> HentAlle()
    {
        return await http.GetFromJsonAsync<List<Bruger>>("api/brugere")
               ?? new List<Bruger>(); // fallback hvis server returnerer null
    }
    
    // Henter alle elever fra backend
    public async Task<List<Bruger>> HentAlleElever()
    {
        return await http.GetFromJsonAsync<List<Bruger>>("api/brugere/elever")
               ?? new List<Bruger>(); // fallback hvis server returnerer null
    }


    // Henter alle køkkenchefer for en specifik lokation (id)
 
    public async Task<List<Bruger>> HentKoekkencheferForLokation(int lokationId)
    {
        var url = $"api/brugere/køkkencheferlokation/{lokationId}";
        var kokke = await http.GetFromJsonAsync<List<Bruger>>(url);
        return kokke ?? new List<Bruger>();
    }
    
    public async Task<List<string>> HentAlleErhverv()
    {
        var erhverv = await http.GetFromJsonAsync<List<string>>("api/brugere/erhverv");
        return erhverv ?? new List<string>();
    }

    public async Task<List<string>> HentAlleKurser()
    {
        var result = await http.GetFromJsonAsync<List<string>>("api/brugere/kurser");
        return result ?? new List<string>();
    }
    
    // Hent elevplan ud fra brugerId
    public async Task<Shared.Elevplan?> HentElevplanForBruger(int brugerId, int forespoergerId)
    {
        var response = await http.GetAsync($"api/brugere/{brugerId}/elevplan?forespoergerId={forespoergerId}");

        if (!response.IsSuccessStatusCode)
        {
            var fejl = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[GetElevplanForUser] FEJL: {fejl}");
            return null;
        }

        return await response.Content.ReadFromJsonAsync<Shared.Elevplan?>();
    }
    
    public async Task OpdaterBruger(Bruger bruger)
    {
        var response = await http.PutAsJsonAsync($"api/brugere/{bruger._id}", bruger);
        response.EnsureSuccessStatusCode(); // fejler med exception hvis noget går galt
    }
    
    // Denne metode opdaterer SkoleId for en specifik praktikperiode i en brugers elevplan
    public async Task OpdaterSkoleId(int brugerId, int periodeIndex, int? nySkoleId)
    {
        // Bygger URL’en til API-kaldet. Eksempel:
        // Hvis nySkoleId er null, sendes 0 for at signalere "ingen skole valgt"
        var url = $"api/brugere/{brugerId}/skole?periodeIndex={periodeIndex}&nySkoleId={(nySkoleId ?? 0)}";

        // Sender en HTTP PUT-request til backend-API’et (body er null – al data ligger i URL’en)
        var response = await http.PutAsync(url, null);

        // Tjekker om serveren returnerede succes (statuskode 200 OK)
        if (!response.IsSuccessStatusCode)
        {
            // Læser fejlbesked fra serveren
            var fejl = await response.Content.ReadAsStringAsync();

            // Logger fejlen i konsollen
            Console.WriteLine($"[OpdaterSkoleId] FEJL: {fejl}");

            // Smider en exception så frontend ved, at opdateringen mislykkedes
            throw new Exception("Kunne ikke opdatere SkoleId.");
        }
    }

    
    public async Task<List<Bruger>> HentFiltreredeElever(string soegeord, string kursus, string erhverv, int? deadline, string rolle, string? status, int? afdelingId, bool? aktiv)
    {
        var url = $"api/brugere/filtreredeelever?soegeord={Uri.EscapeDataString(soegeord)}" +
                  $"&kursus={Uri.EscapeDataString(kursus)}" +
                  $"&erhverv={Uri.EscapeDataString(erhverv)}" +
                  $"&deadline={(deadline.HasValue ? deadline.Value.ToString() : "")}" +
                  $"&rolle={Uri.EscapeDataString(rolle)}" +
                  $"&status={Uri.EscapeDataString(status ?? "")}" +
                  $"&afdelingId={(afdelingId.HasValue ? afdelingId.Value.ToString() : "")}" +
                  $"&aktiv={(aktiv.HasValue ? aktiv.Value.ToString().ToLower() : "")}";
        return await http.GetFromJsonAsync<List<Bruger>>(url) ?? new();
    }

    public async Task<byte[]> EksporterFiltreredeElever(
        string soegeord,
        string kursus,
        string erhverv,
        int? deadline,
        string rolle,
        string? status,
        int? afdelingId,
        bool? aktiv) 
    {
        var url = $"api/brugere/eksporter-elever?" +
                  $"soegeord={Uri.EscapeDataString(soegeord)}" +
                  $"&kursus={Uri.EscapeDataString(kursus ?? "")}" +
                  $"&erhverv={Uri.EscapeDataString(erhverv ?? "")}" +
                  $"&deadline={(deadline.HasValue ? deadline.Value.ToString() : "")}" +
                  $"&rolle={Uri.EscapeDataString(rolle)}" +
                  $"&status={Uri.EscapeDataString(status ?? "")}" +
                  $"&afdelingId={(afdelingId.HasValue ? afdelingId.Value.ToString() : "")}" +
                  $"&aktiv={(aktiv.HasValue ? aktiv.Value.ToString().ToLower() : "")}";

        return await http.GetByteArrayAsync(url);
    }


    
    //Funktion til dynamisk at beregne deadline for et delmål, baseret på DageTilDeadline
    private void BeregnDeadlinesIElevplan(Shared.Elevplan plan)
    {
        foreach (var periode in plan.ListPerioder)
        {
            if (periode.StartDato == null) continue;

            foreach (var maal in periode.ListMaal)
            {
                foreach (var delmaal in maal.ListDelmaal)
                {
                    if (delmaal.DageTilDeadline.HasValue)
                    {
                        delmaal.Deadline = periode.StartDato.Value.AddDays(delmaal.DageTilDeadline.Value);
                    }
                }
            }
        }
    }

}
