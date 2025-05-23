using Backend.Repositories.Interface;
using MongoDB.Driver;
using Shared;

namespace Backend.Repositories;

public class ElevplanRepositoryMongoDB : IElevplanRepository
{
    private IMongoClient client;
    private IMongoCollection<Elevplan> SkabelonCollection;
    private IMongoCollection<Bruger> BrugerCollection;
     

    public ElevplanRepositoryMongoDB()
    {
        // atlas database
        var password = "Comwell";
        var mongoUri = $"mongodb+srv://Comwell:{password}@comwell.mils9ta.mongodb.net/?retryWrites=true&w=majority&appName=Comwell";

        //local mongodb
        //var mongoUri = "mongodb://localhost:27017/";

        try
        {
            client = new MongoClient(mongoUri);
        }
        catch (Exception e)
        {
            Console.WriteLine("There was a problem connecting to your " +
                              "Atlas cluster. Check that the URI includes a valid " +
                              "username and password, and that your IP address is " +
                              $"in the Access List. Message: {e.Message}");
            throw;
        }

        // Provide the name of the database and collection you want to use.
        var dbName = "Comwell";
        var Brugerecollection = "Brugere";
        var Skabeloncollection = "Skabeloner";

        BrugerCollection = client.GetDatabase(dbName)
            .GetCollection<Bruger>(Brugerecollection);
        SkabelonCollection = client.GetDatabase(dbName)
            .GetCollection<Elevplan>(Skabeloncollection);

    }
    
    public async Task<Elevplan?> HentSkabelon(string skabelonNavn)
    {
        //Opretter et filter der matcher det medsendte skabelonNavn, men SkabelonNavn i mongoDB
        var filter = Builders<Elevplan>.Filter.Eq("SkabelonNavn", skabelonNavn);
        return await SkabelonCollection.Find(filter).FirstOrDefaultAsync();
    }
    
    public async Task TilfoejKommentar(int elevplanId, int delmaalId, Kommentar kommentar)
    {
        //opretter et filter der finder den rigtige elevplan, ved at lede efter elevplanID i brugere
        var filter = Builders<Bruger>.Filter.Eq(b => b.MinElevplan._id, elevplanId);
        var bruger = await BrugerCollection.Find(filter).FirstOrDefaultAsync();
        
        if (bruger == null || bruger.MinElevplan == null)
            throw new Exception($"Ingen elevplan med ID {elevplanId} fundet.");
        
        //finder det rigtige delmål, ved at matche delmålId fra det valgte delmål i frontend med det i databasen
        var delmaal = bruger.MinElevplan.ListPerioder
            .SelectMany(p => p.ListMaal)
            .SelectMany(m => m.ListDelmaal)
            .FirstOrDefault(d => d._id == delmaalId);

        if (delmaal == null)
            throw new Exception($"Delmål med ID {delmaalId} blev ikke fundet.");
        
        //Tilføjer kommentaren til listen af kommentarer for delmålet
        delmaal.Kommentar.Add(kommentar);

        //Opdaterer brugeren - dette gøres da kommentarer er dybt embedded og derfor er svære at tilgå
        await BrugerCollection.ReplaceOneAsync(b => b._id == bruger._id, bruger);
    }
    
    public async Task<Kommentar?> GetKommentarAsync(int elevplanId, int delmaalId, string brugerRolle)
    {
        //Finder den rette elevplan, ved at matche elevplanID med det medsendte ID
        var filter = Builders<Bruger>.Filter.Eq(b => b.MinElevplan._id, elevplanId);
        var bruger = await BrugerCollection.Find(filter).FirstOrDefaultAsync();

        if (bruger?.MinElevplan == null)
            return null;

        //Finder det rette delmål, ved at kigge i den fundne elevplans delmål efter det specifikke ID
        var delmaal = bruger.MinElevplan.ListPerioder?
            .SelectMany(p => p.ListMaal)
            .SelectMany(m => m.ListDelmaal)
            .FirstOrDefault(d => d._id == delmaalId);

        if (delmaal == null)
            return null;

        //Returnerer kommentaren som matcher brugeren som er logget ind's rolle - faglært og køkkenchef tæller for 1
        if (brugerRolle == "FaglærtKok" || brugerRolle == "Køkkenchef")
        {
            return delmaal.Kommentar?
                .FirstOrDefault(k => k.OprettetAfRolle == "FaglærtKok" || k.OprettetAfRolle == "Køkkenchef");
        }

        return delmaal.Kommentar?
            .FirstOrDefault(k => k.OprettetAfRolle == brugerRolle);

    }
    
    public async Task RedigerKommentarAsync(int elevplanId, int delmaalId, int kommentarId, Kommentar redigeretKommentar)
    {
        var filter = Builders<Bruger>.Filter.Eq(b => b.MinElevplan._id, elevplanId);
        var bruger = await BrugerCollection.Find(filter).FirstOrDefaultAsync();

        if (bruger?.MinElevplan == null)
            throw new Exception($"Ingen elevplan med ID {elevplanId} fundet.");

        var delmaal = bruger.MinElevplan.ListPerioder
            .SelectMany(p => p.ListMaal)
            .SelectMany(m => m.ListDelmaal)
            .FirstOrDefault(d => d._id == delmaalId);

        if (delmaal == null)
            throw new Exception($"Delmål med ID {delmaalId} ikke fundet.");

        var kommentar = delmaal.Kommentar.FirstOrDefault(k => k._id == kommentarId);

        if (kommentar == null)
            throw new Exception($"Kommentar med ID {kommentarId} ikke fundet.");

        // Opdaterer hele kommentaren
        kommentar.Tekst = redigeretKommentar.Tekst;
        kommentar.Dato = DateOnly.FromDateTime(DateTime.Today);
        kommentar.KommentarBillede = redigeretKommentar.KommentarBillede;

        // Opdater databasen
        await BrugerCollection.ReplaceOneAsync(b => b._id == bruger._id, bruger);
    }

    
    public async Task OpdaterStatusAsync(int elevplanId, Delmaal delmaal)
    {
        //Finder den rette elevplan, ved at søge efter elevplanID
        var filter = Builders<Bruger>.Filter.Eq(b => b.MinElevplan._id, elevplanId);
        var bruger = await BrugerCollection.Find(filter).FirstOrDefaultAsync();

        if (bruger?.MinElevplan == null)
            throw new Exception("Elevplan ikke fundet");
        
        //finder det delmål som skal have skiftet sin status, ved at matche delmålID med det som sendes med
        var _delmaal = bruger.MinElevplan.ListPerioder
            .SelectMany(p => p.ListMaal)
            .SelectMany(m => m.ListDelmaal)
            .FirstOrDefault(d => d._id == delmaal._id);

        if (_delmaal == null)
            throw new Exception("Delmål ikke fundet");
        
        //Sætter de nye felter ind i det fundne delmål
        _delmaal.Status = delmaal.Status;
        _delmaal.StatusLog = delmaal.StatusLog;
        
        //Opdaterer brugeren, for at gemme den nye status
        await BrugerCollection.ReplaceOneAsync(b => b._id == bruger._id, bruger);
    }


    // Returnerer mål med filtre og søgning – bruges til elevplanvisning

    public async Task<List<Maal>> HentFiltreredeMaal(
        int brugerId,
        int periodeIndex,
        string? valgtMaalNavn,
        string? valgtDelmaalType,
        string? soegeord,
        string? filterStatus)
    {
        // Finder brugeren baseret på brugerId
        var bruger = await BrugerCollection.Find(b => b._id == brugerId).FirstOrDefaultAsync();

        // Hvis brugeren ikke har nogen elevplan
        if (bruger?.MinElevplan == null)
            return new List<Maal>();

        var plan = bruger.MinElevplan;

        // Sikrer at periodeIndex er gyldigt
        if (periodeIndex < 0 || periodeIndex >= plan.ListPerioder.Count)
            return new List<Maal>();




        var periode = plan.ListPerioder[periodeIndex];
        var søg = soegeord?.ToLower() ?? "";

        // Gennemgår alle mål i perioden og anvender filtre
        return periode.ListMaal
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
            .Where(m => m.ListDelmaal.Any()) // Kun mål der har delmål tilbage efter filtrering
            .ToList();
    }
    public async Task TilfoejDelmaal(int elevplanId, int maalId, Delmaal nytDelmaal)
    {
        var filter = Builders<Bruger>.Filter.Eq(b => b.MinElevplan._id, elevplanId);
        var bruger = await BrugerCollection.Find(filter).FirstOrDefaultAsync();

        if (bruger?.MinElevplan == null)
            throw new Exception($"Ingen elevplan med ID {elevplanId} fundet.");

        var maal = bruger.MinElevplan.ListPerioder
            .SelectMany(p => p.ListMaal)
            .FirstOrDefault(m => m._id == maalId);

        if (maal == null)
            throw new Exception($"Mål med ID {maalId} ikke fundet.");

        // Her er ID allerede sat af frontend/service
        maal.ListDelmaal.Add(nytDelmaal);
        
        if (string.IsNullOrWhiteSpace(nytDelmaal.DeadlineKommentar) && !nytDelmaal.Deadline.HasValue)
            throw new Exception("Enten en deadline-dato eller en kommentar skal være udfyldt.");

        // Sæt DageTilDeadline hvis der er en deadline
        if (nytDelmaal.Deadline.HasValue)
        {
            var iDag = DateOnly.FromDateTime(DateTime.Today);
            nytDelmaal.DageTilDeadline = (nytDelmaal.Deadline.Value.DayNumber - iDag.DayNumber);
        }


        await BrugerCollection.ReplaceOneAsync(b => b._id == bruger._id, bruger);
    }
    
    public async Task OpdaterDelmaal(int elevplanId, int periodeIndex, int maalId, int delmaalId, Delmaal opdateretDelmaal)
    {
        var filter = Builders<Bruger>.Filter.Eq(b => b._id, elevplanId);
        var bruger = await BrugerCollection.Find(filter).FirstOrDefaultAsync();
        var plan = bruger?.MinElevplan;

        if (plan == null)
            throw new Exception("Elevplan ikke fundet.");

        var periode = plan.ListPerioder.ElementAtOrDefault(periodeIndex);
        if (periode == null)
            throw new Exception("Periode ikke fundet.");

        var maal = periode.ListMaal.FirstOrDefault(m => m._id == maalId);
        if (maal == null)
            throw new Exception("Mål ikke fundet.");

        var delmaalIndex = maal.ListDelmaal.FindIndex(d => d._id == delmaalId);
        if (delmaalIndex == -1)
            throw new Exception("Delmål ikke fundet.");

        maal.ListDelmaal[delmaalIndex] = opdateretDelmaal;

        await BrugerCollection.ReplaceOneAsync(filter, bruger);
    }

    public async Task<bool> SletDelmaal(int elevplanId, int periodeIndex, int maalId, int delmaalId)
    {
        var filter = Builders<Bruger>.Filter.Eq(b => b._id, elevplanId);
        var bruger = await BrugerCollection.Find(filter).FirstOrDefaultAsync();
        var plan = bruger?.MinElevplan;

        if (plan == null)
            throw new Exception("Elevplan ikke fundet.");

        var periode = plan.ListPerioder.ElementAtOrDefault(periodeIndex);
        if (periode == null)
            throw new Exception("Periode ikke fundet.");

        var maal = periode.ListMaal.FirstOrDefault(m => m._id == maalId);
        if (maal == null)
            throw new Exception("Mål ikke fundet.");

        var fjernet = maal.ListDelmaal.RemoveAll(d => d._id == delmaalId);
        if (fjernet == 0)
            throw new Exception("Delmål ikke fundet eller kunne ikke fjernes.");

        await BrugerCollection.ReplaceOneAsync(filter, bruger);
        return true;
    }

    
    public async Task<Elevplan?> HentElevplanMedMaal(int elevplanId, int periodeIndex)
    {
        var filter = Builders<Bruger>.Filter.Eq(b => b.MinElevplan._id, elevplanId);
        var bruger = await BrugerCollection.Find(filter).FirstOrDefaultAsync();
        return bruger?.MinElevplan;
    }
    public async Task OpdaterIgangAsync(int elevplanId, Delmaal delmaal)
    {
        var filter = Builders<Bruger>.Filter.Eq(b => b.MinElevplan._id, elevplanId);
        var bruger = await BrugerCollection.Find(filter).FirstOrDefaultAsync();

        if (bruger?.MinElevplan == null)
            throw new Exception("Elevplan ikke fundet");

        var _delmaal = bruger.MinElevplan.ListPerioder
            .SelectMany(p => p.ListMaal)
            .SelectMany(m => m.ListDelmaal)
            .FirstOrDefault(d => d._id == delmaal._id);

        if (_delmaal == null)
            throw new Exception("Delmål ikke fundet");

        _delmaal.Igang = delmaal.Igang;

        await BrugerCollection.ReplaceOneAsync(b => b._id == bruger._id, bruger);
    }

}
