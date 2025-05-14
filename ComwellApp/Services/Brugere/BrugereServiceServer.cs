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
    public async Task TilfoejElev(Bruger nyBruger, Bruger ansvarlig, string skabelonType)
    {
        // Henter skabelon fra vores backend metode (elevplans controller)
        var response = await http.GetAsync($"api/elevplan/skabelon/{skabelonType}");
        if (!response.IsSuccessStatusCode)
        {
            var fejl = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[TilfoejElev] FEJL: {fejl}");
            throw new Exception($"Fejl ved hentning af skabelon '{skabelonType}'");
        }
        
        //Konvereterer vores svar fra api til et Elevplan objekt
        var plan = await response.Content.ReadFromJsonAsync<Shared.Elevplan>();
        if (plan == null)
            throw new Exception("Skabelonen kunne ikke konverteres til Elevplan");

        //Tilpasser elevplanen til den enkelte elev ved at give ID, sætte den ansvarlige og tømme alle statusfelter
        plan.ElevplanId = _idGenerator.GenererNytId(_allePlaner, p => p.ElevplanId);
        plan.Ansvarlig = ansvarlig;

        foreach (var periode in plan.ListPerioder)
        {
            foreach (var maal in periode.ListMaal)
            {
                foreach (var delmaal in maal.ListDelmaal)
                {
                    delmaal.Status = false;

                    foreach (var opgave in delmaal.ListOpgaver)
                    {
                        opgave.OpgaveGennemfoert = false;
                    }
                }
            }
        }

        //Tilføjer elevolanen til den nye bruger
        nyBruger.MinElevplan = plan;

        //Kalder vores controller, og sender vores nye bruger med
        var postResponse = await http.PostAsJsonAsync("api/brugere", nyBruger);
        if (!postResponse.IsSuccessStatusCode)
        {
            var fejl = await postResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"[TilfoejElev] FEJL ved oprettelse: {fejl}");
            throw new Exception("Kunne ikke oprette bruger med elevplan");
        }
    }


}