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

    public async Task<List<Maal>> HentFiltreredeMaal(int brugerId, int periodeIndex, string? valgtMaalNavn, string? valgtDelmaalType, string? soegeord, bool? filterStatus)
    {
        // Saml query-parametre som URL
        string url = $"api/elevplan/filtrerede-maal" +
                     $"?brugerId={brugerId}" +
                     $"&periodeIndex={periodeIndex}" +
                     $"&maalNavn={Uri.EscapeDataString(valgtMaalNavn ?? "")}" +
                     $"&delmaalType={Uri.EscapeDataString(valgtDelmaalType ?? "")}" +
                     $"&soegeord={Uri.EscapeDataString(soegeord ?? "")}" +
                     $"&status={(filterStatus.HasValue ? filterStatus.Value.ToString().ToLower() : "")}";
        try
        {
            var response = await http.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var fejl = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[HentFiltreredeMaal] FEJL: {fejl}");
                throw new Exception("Kunne ikke hente filtrerede mål fra server.");
            }

            var maalListe = await response.Content.ReadFromJsonAsync<List<Maal>>();
            return maalListe ?? new();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[HentFiltreredeMaal] FEJL: {ex.Message}");
            return new();
        }
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

        //Tvinger rollen til at være faglærtkok, selvom det er køkkenchef der opretter
        //Dette er for at sikre at de to har samme kommentarfelt
        if (nyKommentar.OprettetAf?.Rolle == "Køkkenchef")
        {
            nyKommentar.OprettetAf.Rolle = "FaglærtKok";
        }

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
    
    public async Task RedigerKommentar(Elevplan minPlan, int delmaalId, int kommentarId, string nyTekst)
    {
        //Kalder vores controller for at sende den nye kommentar
        var response = await http.PutAsJsonAsync(
            $"api/elevplan/kommentar/{minPlan.ElevplanId}/{delmaalId}/{kommentarId}",
            nyTekst);

        //fejlhåndtering
        if (!response.IsSuccessStatusCode)
        {
            var fejl = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[RedigerKommentar] FEJL: {fejl}");
            throw new Exception("Kunne ikke redigere kommentar.");
        }
    }

    public async Task<Kommentar?> GetKommentarAsync(int elevplanId, int delmaalId, string brugerRolle)
    {
        try
        {
            var response = await http.GetAsync($"api/elevplan/kommentar/{elevplanId}/{delmaalId}/{brugerRolle}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Kommentar>();
            }

            Console.WriteLine($"[GetKommentarAsync] Kommentar ikke fundet. Statuskode: {response.StatusCode}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GetKommentarAsync] Fejl: {ex.Message}");
            return null;
        }
    }
    
    public async Task OpdaterStatus(Elevplan plan, Delmaal delmaal)
    {
        //Kalder vores controller, og sender det rigtige delmål med - hvor status er opdateret
        var response = await http.PutAsJsonAsync($"api/elevplan/statusopdatering/{plan.ElevplanId}", delmaal);

        if (!response.IsSuccessStatusCode)
        {
            var fejl = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[OpdaterStatus] FEJL: {fejl}");
            throw new Exception("Kunne ikke opdatere status");
        }
    }


    
    
}