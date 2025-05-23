using ComwellApp.Pages;
using Shared;
using Shared.ViewModeller;

namespace ComwellApp.Services.Elevplan;

public class ElevplanServiceMock : IElevplanService
{
    private readonly IdGeneratorService _idGenerator;

    public ElevplanServiceMock(IdGeneratorService idGenerator)
    {
        _idGenerator = idGenerator;
    }

    private List<Shared.Elevplan> alleElevplaner = new();

    public Task TilfoejKommentar(Shared.Elevplan minPlan, int delmaalId, Kommentar nyKommentar)
    {
        //Her finder vi alle delmål i den elevplan der sendes med og derefter det specifikke delmål ud fra ID
        var delmaal = minPlan.ListPerioder
            .SelectMany(p => p.ListMaal)
            .SelectMany(m => m.ListDelmaal)
            .FirstOrDefault(d => d._id == delmaalId);

        if (delmaal != null)
        {
            //Genererer et ID til den nye kommentar via vores service
            nyKommentar._id = _idGenerator.GenererNytId(delmaal.Kommentar, k => k._id);
            nyKommentar.Dato = DateOnly.FromDateTime(DateTime.Today);

            //kommentar tilføjes
            delmaal.Kommentar.Add(nyKommentar);
        }

        return Task.CompletedTask;
    }
    
    public Task RedigerKommentar(Shared.Elevplan minPlan, int delmaalId, Kommentar redigeretKommentar)
    {
        throw new NotImplementedException();
    }

    public Task<Kommentar?> GetKommentarAsync(int elevplanId, int delmaalId, string brugerRolle)
    {
        throw new NotImplementedException();
    }

    public Task OpdaterStatus(Shared.Elevplan plan, Delmaal delmaal)
    {
        throw new NotImplementedException();
    }

    public Task<Shared.Elevplan> LavDefaultSkabelon(Bruger ansvarlig, string skabelonNavn)
    {
        throw new NotImplementedException();
    }

    public Task<Shared.Elevplan> LavDefaultSkabelon(Bruger ansvarlig, string skabelonNavn, DateOnly startdato)
    {
        throw new NotImplementedException();
    }

    public Task<List<Maal>> HentFiltreredeMaal(int brugerId, int periodeIndex, string? valgtMaalNavn,
        string? valgtDelmaalType, string? soegeord, string? filterStatus)
    {
        var plan = alleElevplaner.FirstOrDefault(p => p._id == brugerId);
        if (plan == null || plan.ListPerioder.Count <= periodeIndex)
            return Task.FromResult(new List<Maal>());

        var periode = plan.ListPerioder[periodeIndex];
        var søg = soegeord?.ToLower() ?? "";

        var result = periode.ListMaal
            .Where(m => string.IsNullOrWhiteSpace(valgtMaalNavn) || m.MaalNavn == valgtMaalNavn)
            .Select(m => new Maal
            {
                _id = m._id,
                MaalNavn = m.MaalNavn,
                ListDelmaal = m.ListDelmaal
                    .Where(d =>
                        (string.IsNullOrWhiteSpace(valgtDelmaalType) || d.DelmaalType == valgtDelmaalType) &&
                        (string.IsNullOrWhiteSpace(søg) || d.Titel.ToLower().Contains(søg)) &&
                        (filterStatus == null
                         || (filterStatus == "true" && d.Status)
                         || (filterStatus == "false" && !d.Status)
                         || (filterStatus == "igang" && d.Igang && !d.Status))
                    )
                    .ToList()
            })
            .Where(m => m.ListDelmaal.Any())
            .ToList();

        return Task.FromResult(result);
    }


    //Opretter en ny elevplan, ved at kalde vores skabelon funktion og sende bruger + ansvarlig med
    public async Task<Shared.Elevplan> OpretElevplan(Bruger ansvarlig, string skabelonNavn)
    {
        var plan = await LavDefaultSkabelon(ansvarlig, skabelonNavn);
        plan._id = _idGenerator.GenererNytId(alleElevplaner, p => p._id);
        alleElevplaner.Add(plan);
        return plan;
    }

    //Returnerer vores skabelon, og tilknytter den medsendte elev og ansvarlige fra OpretElevplan() til elev og ansvarlig felter
    

    public Task<List<Maal>> HentFiltreredeMaal(int brugerId, int periodeIndex, string? valgtMaalNavn,
        string? valgtDelmaalType,
        string? soegeord, bool? filterStatus)
    {
        throw new NotImplementedException();
    }

    public Task TilfoejDelmaal(Shared.Elevplan plan, int maalId, Delmaal nytDelmaal)
    {
        throw new NotImplementedException();
    }

    public Task OpdaterDelmaal(Shared.Elevplan plan, int periodeIndex, int maalId, Delmaal opdateretDelmaal)
    {
        throw new NotImplementedException();
    }

    public Task SletDelmaal(Shared.Elevplan plan, int periodeIndex, int maalId, int delmaalId)
    {
        throw new NotImplementedException();
    }

    public Task<List<Maal>> HentMaalFraPeriode(int elevplanId, int periodeIndex)
    {
        throw new NotImplementedException();
    }

    public Task<List<string>> HentDelmaalTyperFraPeriode(int elevplanId, int periodeIndex)
    {
        throw new NotImplementedException();
    }

    public Task OpdaterIgang(Shared.Elevplan plan, Delmaal delmaal)
    {
        throw new NotImplementedException();
    }

    public Task<List<DelmaalDeadlineVisning>> HentDeadlinesSomVisning(int brugerId)
    {
        throw new NotImplementedException();
    }
}