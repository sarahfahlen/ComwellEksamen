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

    public List<Maal> HentFiltreredeMaal(Elevplan plan, int periodeIndex, string? valgtMaalNavn, string? valgtDelmaalType,
        string? søgeord, bool? filterStatus)
    {
        throw new NotImplementedException();
    }

    public async Task TilfoejKommentar(Elevplan minPlan, int delmaalId, Kommentar nyKommentar)
    {
        //Finder det rigtige delmål for at vores Idgenerator kan lave et korrekt Id til den nye kommentar
        var delmaal = minPlan.ListPerioder
            .SelectMany(p => p.ListMaal)
            .SelectMany(m => m.ListDelmaal)
            .FirstOrDefault(d => d.DelmaalId == delmaalId);

        if (delmaal == null)
            throw new Exception("Delmål ikke fundet i planen.");

        //Opretter Id til den nye kommentar, og sætter dagens dato på
        nyKommentar.KommentarId = _idGenerator.GenererNytId(delmaal.Kommentarer, k => k.KommentarId);
        nyKommentar.Dato = DateOnly.FromDateTime(DateTime.Today);

        //Kalder vores controller og forsøger at gemme den nye kommentar
        var response = await http.PostAsJsonAsync(
            $"api/elevplan/kommentar/{minPlan.ElevplanId}/{delmaalId}",
            nyKommentar);

        //Fejlhåndtering
        if (!response.IsSuccessStatusCode)
        {
            var fejl = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[TilfoejKommentar] FEJL: {fejl}");
            throw new Exception("Kunne ikke tilføje kommentar");
        }
    }

    public Task RedigerKommentar(Elevplan minPlan, int delmaalId, int kommentarId, string nyTekst)
    {
        throw new NotImplementedException();
    }

    public Kommentar? GetKommentar(Elevplan plan, int delmaalId, string brugerRolle)
    {
        throw new NotImplementedException();
    }

    public List<Elevplan> GetAllElevplaner()
    {
        throw new NotImplementedException();
    }
    
}