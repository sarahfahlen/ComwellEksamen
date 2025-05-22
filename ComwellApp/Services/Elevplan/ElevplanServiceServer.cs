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

    public async Task<Elevplan> LavDefaultSkabelon(Bruger ansvarlig, string skabelonNavn, DateOnly startdato)
    {
        var response = await http.GetAsync($"api/elevplan/skabelon/{skabelonNavn}");
        if (!response.IsSuccessStatusCode)
        {
            var fejl = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[LavDefaultSkabelon] FEJL: {fejl}");
            throw new Exception($"Fejl ved hentning af skabelon '{skabelonNavn}'");
        }

        var skabelon = await response.Content.ReadFromJsonAsync<Elevplan>();
        if (skabelon == null)
            throw new Exception("Skabelonen kunne ikke konverteres til Elevplan");

        skabelon.Ansvarlig = ansvarlig;
        skabelon.ElevStartDato = startdato;

        // 🟡 Nu vil vi kun sætte datoer på – IKKE overskrive perioderne
        foreach (var periode in skabelon.ListPerioder ?? new())
        {
            if (periode.Praktikvarighed > 0)
            {
                periode.StartDato = startdato;
                periode.SlutDato = startdato.AddDays(periode.Praktikvarighed * 7 - 1);
                startdato = periode.SlutDato.Value.AddDays(1); // næste periodes start
            }
        }

        return skabelon;
    }


    private List<Praktikperiode> GenererPraktikperioder(DateOnly startdato)
    {
        var perioder = new List<Praktikperiode>();
        var længderIuger = new[] { 52, 43, 43, 4 };
        var start = startdato;

        for (int i = 0; i < længderIuger.Length; i++)
        {
            var uger = længderIuger[i];
            var slut = start.AddDays(uger * 7 - 1);

            perioder.Add(new Praktikperiode
            {
                PraktikNavn = $"Periode {i + 1}",
                Praktikvarighed = uger,
                StartDato = start,
                SlutDato = slut,
                ListMaal = new List<Maal>()
            });

            start = slut.AddDays(1);
        }

        return perioder;
    }


    public async Task<List<Maal>> HentFiltreredeMaal(int brugerId, int periodeIndex, string? valgtMaalNavn,
        string? valgtDelmaalType, string? soegeord, bool? filterStatus)
    {
        // Saml query-parametre som URL
        string url = $"api/elevplan/filtreredemaal" +
                     $"?brugerId={brugerId}" +
                     $"&periodeIndex={periodeIndex}" +
                     $"&valgtMaalNavn={Uri.EscapeDataString(valgtMaalNavn ?? "")}" +
                     $"&valgtDelmaalType={Uri.EscapeDataString(valgtDelmaalType ?? "")}" +
                     $"&soegeord={Uri.EscapeDataString(soegeord ?? "")}" +
                     $"&filterStatus={(filterStatus.HasValue ? filterStatus.Value.ToString().ToLower() : "")}";
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
            .FirstOrDefault(d => d._id == delmaalId);

        if (delmaal == null)
            throw new Exception("Delmål ikke fundet i planen.");

        //Tvinger rollen til at være faglærtkok, selvom det er køkkenchef der opretter
        //Dette er for at sikre at de to har samme kommentarfelt
        if (nyKommentar.OprettetAfRolle == "Køkkenchef")
        {
            nyKommentar.OprettetAfRolle = "FaglærtKok";
        }

        //Opretter Id til den nye kommentar, og sætter dagens dato på
        nyKommentar._id = _idGenerator.GenererNytId(delmaal.Kommentar, k => k._id);
        nyKommentar.Dato = DateOnly.FromDateTime(DateTime.Today);

        //Kalder vores controller og forsøger at gemme den nye kommentar
        var response = await http.PostAsJsonAsync(
            $"api/elevplan/kommentar/{minPlan._id}/{delmaalId}",
            nyKommentar);

        //Fejlhåndtering
        if (!response.IsSuccessStatusCode)
        {
            var fejl = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[TilfoejKommentar] FEJL: {fejl}");
            throw new Exception("Kunne ikke tilføje kommentar");
        }
    }

    public async Task RedigerKommentar(Elevplan minPlan, int delmaalId, Kommentar redigeretKommentar)
    {
        var response = await http.PutAsJsonAsync(
            $"api/elevplan/kommentar/{minPlan._id}/{delmaalId}/{redigeretKommentar._id}",
            redigeretKommentar);

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
        var response = await http.PutAsJsonAsync($"api/elevplan/statusopdatering/{plan._id}", delmaal);

        if (!response.IsSuccessStatusCode)
        {
            var fejl = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[OpdaterStatus] FEJL: {fejl}");
            throw new Exception("Kunne ikke opdatere status");
        }
    }

    public async Task TilfoejDelmaal(Elevplan plan, int maalId, Delmaal nytDelmaal)
    {
        var maal = plan.ListPerioder
            .SelectMany(p => p.ListMaal)
            .FirstOrDefault(m => m._id == maalId);

        if (maal == null)
            throw new Exception($"Mål med ID {maalId} ikke fundet.");

        // Generer ID til delmål
        nytDelmaal._id = _idGenerator.GenererNytDelmaalId(plan);
        nytDelmaal.Status = false;

        // Generer ID til opgaver og marker som ikke gennemført
        foreach (var opg in nytDelmaal.ListOpgaver)
        {
            opg._id = _idGenerator.GenererNytId(nytDelmaal.ListOpgaver, d => d._id);
            opg.OpgaveGennemfoert = false;
        }

        var response = await http.PostAsJsonAsync(
            $"api/elevplan/delmaal/{plan._id}/{maalId}",
            nytDelmaal);

        if (!response.IsSuccessStatusCode)
        {
            var fejl = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[TilfoejDelmaalAsync] FEJL: {fejl}");
            throw new Exception("Kunne ikke tilføje delmål.");
        }
    }

    public async Task OpdaterDelmaal(Elevplan plan, int periodeIndex, int maalId, Delmaal opdateretDelmaal)
    {
        var response = await http.PutAsJsonAsync(
            $"api/elevplan/delmaal/{plan._id}/{periodeIndex}/{maalId}/{opdateretDelmaal._id}",
            opdateretDelmaal);

        if (!response.IsSuccessStatusCode)
        {
            var fejl = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[OpdaterDelmaal] FEJL: {fejl}");
            throw new Exception("Kunne ikke opdatere delmål.");
        }
    }


    public async Task<List<Maal>> HentMaalFraPeriode(int elevplanId, int periodeIndex)
    {
        var response = await http.GetAsync($"api/elevplan/maal/{elevplanId}/{periodeIndex}");
        return await response.Content.ReadFromJsonAsync<List<Maal>>() ?? new();
    }

    public async Task<List<string>> HentDelmaalTyperFraPeriode(int elevplanId, int periodeIndex)
    {
        var response = await http.GetAsync($"api/elevplan/delmaaltyper/{elevplanId}/{periodeIndex}");
        return await response.Content.ReadFromJsonAsync<List<string>>() ?? new();
    }

    public async Task<List<Delmaal>> HentKommendeDeadlines(int brugerId)
    {
        var response = await http.GetAsync($"api/elevplan/kommendedeadlines/{brugerId}");

        if (!response.IsSuccessStatusCode)
        {
            var fejl = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[HentKommendeDeadlines] FEJL: {fejl}");
            return new();
        }

        return await response.Content.ReadFromJsonAsync<List<Delmaal>>() ?? new();
    }

    public async Task OpdaterIgang(Elevplan plan, Delmaal delmaal)
    {
        var response = await http.PutAsJsonAsync($"api/elevplan/igangopdatering/{plan._id}", delmaal);

        if (!response.IsSuccessStatusCode)
        {
            var fejl = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[OpdaterIgang] FEJL: {fejl}");
            throw new Exception("Kunne ikke opdatere 'Igang' status.");
        }
    }
}