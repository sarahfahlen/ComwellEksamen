using System.Net.Http.Json;
using Shared;
namespace ComwellApp.Services.Elevplan;
using Elevplan = Shared.Elevplan;

public class ElevplanServiceServer : IElevplanService
{
    private readonly HttpClient http;
    private readonly IdGeneratorService _idGenerator;

    public ElevplanServiceServer(HttpClient http, IdGeneratorService idGenerator)
    {
        this.http = http;
        _idGenerator = idGenerator;
    }

    private List<Elevplan> _alleElevplaner = new();
    
    public async Task<Elevplan> LavDefaultSkabelon(Bruger ansvarlig, string skabelonNavn)
    {
        //Kalder vores controller med det skabelonNavn der bruges (type af skabelon)
        var response = await http.GetAsync($"api/elevplan/skabelon/{skabelonNavn}");

        //fejlhåndtering
        if (!response.IsSuccessStatusCode)
        {
            var fejl = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[LavDefaultSkabelon] FEJL: {fejl}");
            throw new Exception($"Kunne ikke hente skabelon '{skabelonNavn}' fra server");
        }
        
        //Omdanner skabelon til et objekt af typen Elevplan ved hjælp af Json
        var skabelon = await response.Content.ReadFromJsonAsync<Elevplan>();

        if (skabelon == null)
            throw new Exception("Skabelon kunne ikke læses som elevplan");

        skabelon.Ansvarlig = ansvarlig;

        return skabelon;
    }
    
    public async Task<Elevplan> OpretElevplan(Bruger ansvarlig, string skabelonNavn)
    {
        //henter den rigtige skabelon og tildeler den et nyt ID fra vores service 
        var plan = await  LavDefaultSkabelon(ansvarlig, skabelonNavn);
        plan.ElevplanId = _idGenerator.GenererNytId(_alleElevplaner, p => p.ElevplanId);
        //Sørger for at alle delmål og opgaver sættes til ikke at være fuldført 
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
        _alleElevplaner.Add(plan);
        return plan;
    }
}