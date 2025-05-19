using System.Net.Http.Json;
using Shared;

namespace ComwellApp.Services.Brugere;

// Dette er implementation af IBrugereService, som sender data frem og tilbage med backend.
public class BrugereServiceServer : IBrugereService
{
    private readonly HttpClient http; // Bruges til at sende HTTP-requests til vores backend
    private readonly IdGeneratorService _idGenerator; // Bruges til at generere unikke ID’er til fx elevplaner, delmål og elever
    private static readonly List<Bruger> _brugere = new();
    public BrugereServiceServer(HttpClient http, IdGeneratorService idGenerator)
    {
        this.http = http;
        _idGenerator = idGenerator;
    }

    // Interne lister (bliver dog ikke rigtig brugt her)
    private List<Shared.Elevplan> _allePlaner = new();
    private List<Bruger> _alleBrugere = new();

   
    // Oprettelse af ny elev + automatisk elevplan
   
    public async Task TilfoejElev(Bruger nyBruger, Bruger ansvarlig, string skabelonType)
    {
        // Først henter vi ALLE brugere, for at kunne generere et unikt BrugerId
        var alleBrugere = await HentAlle();
        nyBruger.BrugerId = _idGenerator.GenererNytId(alleBrugere, b => b.BrugerId);

        // Henter elevplan-skabelon fra backend (fx "KokSkabelon")
        var response = await http.GetAsync($"api/elevplan/skabelon/{skabelonType}");
        if (!response.IsSuccessStatusCode)
        {
            // Hvis der er fejl, læs besked fra backend og vis den i konsollen
            var fejl = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[TilfoejElev] FEJL: {fejl}");
            throw new Exception($"Fejl ved hentning af skabelon '{skabelonType}'");
        }

        // Konverterer JSON-svaret til en rigtig .NET Elevplan
        var plan = await response.Content.ReadFromJsonAsync<Shared.Elevplan>();
        if (plan == null)
            throw new Exception("Skabelonen kunne ikke konverteres til Elevplan");

        // Gør planen klar: tildel ansvarlig, elevplanId, sætter startperiode for praktik og beregner deadlines dynamisk
        plan.ElevplanId = nyBruger.BrugerId;
        plan.Ansvarlig = ansvarlig;
        if (nyBruger.StartDato != null && plan.ListPerioder?.Count > 0)
        {
            plan.ListPerioder[0].StartDato = nyBruger.StartDato;
        }
        BeregnDeadlinesIElevplan(plan);

        //  Gennemgår ALLE mål, delmål og opgaver og giver dem unikke ID'er og sætter status til "ikke gennemført"
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

   
    // Henter alle brugere fra backend (kun elever)

    public async Task<List<Bruger>> HentAlle()
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

  
    // Henter alle lokationer, som bruges i dropdown
   
    public async Task<List<Lokation>> HentAlleLokationer()
    {
        var result = await http.GetFromJsonAsync<List<Lokation>>("api/brugere/lokationer");
        return result ?? new List<Lokation>();
    }
    
    public async Task<List<string>> HentAlleErhverv()
    {
        var erhverv = await http.GetFromJsonAsync<List<string>>("api/brugere/erhverv");
        return erhverv ?? new List<string>();
    }

    // Hent elevplan ud fra brugerId
    public async Task<Shared.Elevplan?> HentElevplanForBruger(int brugerId)
    {
        var response = await http.GetAsync($"api/brugere/{brugerId}/elevplan");

        if (!response.IsSuccessStatusCode)
        {
            var fejl = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[GetElevplanForUser] FEJL: {fejl}");
            return null;
        }

        return await response.Content.ReadFromJsonAsync<Shared.Elevplan>();
    }
    
    public async Task<List<Bruger>> HentFiltreredeElever(string soegeord, string lokation, string kursus, string erhverv, int? deadline, string rolle, string? brugerLokation)
    {
        var url = $"api/brugere/filtreredeelever?soegeord={Uri.EscapeDataString(soegeord)}" +
                  $"&lokation={Uri.EscapeDataString(lokation)}" +
                  $"&kursus={Uri.EscapeDataString(kursus)}" +
                  $"&erhverv={Uri.EscapeDataString(erhverv)}" +
                  $"&deadline={(deadline.HasValue ? deadline.Value.ToString() : "")}" +
                  $"&rolle={Uri.EscapeDataString(rolle)}" +
                  $"&brugerLokation={Uri.EscapeDataString(brugerLokation ?? "")}";

        return await http.GetFromJsonAsync<List<Bruger>>(url) ?? new();
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
