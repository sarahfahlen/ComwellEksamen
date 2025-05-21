using Backend.Controllers;
using Backend.Repositories.Interface;
using MongoDB.Driver;
using Shared;

namespace Backend.Repositories;


// Denne klasse håndterer al kommunikation med databasen (MongoDB).
// Her arbejder vi direkte med "Brugere"-collectionen.
// Det er herfra vi fx sender data videre til controllerne, og det er det frontend bruger via services.

public class BrugereRepositoryMongoDB : IBrugereRepository
{
    private IMongoClient client; // Klienten der opretter forbindelse til MongoDB-serveren (Atlas eller lokal)
    private IMongoCollection<Bruger> BrugerCollection; // Collection = tabel i MongoDB – her arbejder vi med brugere


    //Konstruktøren kører når objektet bliver oprettet – her opretter vi databasen og samlingen.
    public BrugereRepositoryMongoDB()
    {
        // Atlas-forbindelse – adgang via brugernavn og kode til cloud MongoDB (Atlas)
        var password = "Comwell";
        var mongoUri = $"mongodb+srv://Comwell:{password}@comwell.mils9ta.mongodb.net/?retryWrites=true&w=majority&appName=Comwell";


        // var mongoUri = "mongodb://localhost:27017/";

        try
        {
            // Prøver at oprette forbindelsen
            client = new MongoClient(mongoUri);
        }
        catch (Exception e)
        {
            // Hvis noget går galt (forkert kode, manglende IP-whitelist, mm.)
            Console.WriteLine("Der opstod en fejl ved forbindelsen til MongoDB. Tjek brugernavn/adgangskode og IP-whitelist.");
            Console.WriteLine($"Fejl: {e.Message}");
            throw; // Stop programmet – vi kan ikke fortsætte uden forbindelse
        }

        // Navnet på vores database og "tabel" i MongoDB
        var dbName = "Comwell";
        var collectionName = "Brugere";

        // Nu henter vi selve collectionen vi skal arbejde med – altså listen over brugere
        BrugerCollection = client.GetDatabase(dbName)
            .GetCollection<Bruger>(collectionName);
    }

 
    // Tilføjer en ny bruger til databasen.
    // Bruges når vi kalder `TilfoejElev(...)` i frontendens BrugereServiceServer.
    public async Task TilfoejElev(Bruger nyBruger)
    {
        await BrugerCollection.InsertOneAsync(nyBruger);
    }
    
    // Henter alle brugere – uanset rolle.
    public async Task<List<Bruger>> HentAlle()
    {
        var filter = Builders<Bruger>.Filter.Empty; // = hent alle dokumenter i collectionen
        return await BrugerCollection.Find(filter).ToListAsync();
    }

   
    // Henter alle elever – altså brugere med rollen "Elev".
    // Bruges i dashboard og elevplan-visninger hvor kun elever vises.
    public async Task<List<Bruger>> HentAlleElever()
    {
        var filter = Builders<Bruger>.Filter.Eq(b => b.Rolle, "Elev"); // kun der hvor Rolle == "Elev"
        return await BrugerCollection.Find(filter).ToListAsync();
    }

    // Henter alle brugere med rollen "Køkkenchef".
    // Bruges fx når man skal vælge en ansvarlig køkkenchef i opret-elev formularen.

    public async Task<List<Bruger>> HentAlleKøkkenchefer()
    {
        var filter = Builders<Bruger>.Filter.Eq(b => b.Rolle, "Køkkenchef"); // kun brugere med denne rolle
        return await BrugerCollection.Find(filter).ToListAsync();
    }

    
    // Henter alle unikke lokationer fra brugernes tilknyttede afdelinger.
    // Bruges fx i frontend, hvor man vælger hvilket køkken eller afdeling eleven skal tilknyttes.
    public async Task<List<Lokation>> HentAlleLokationer()
    {
        // Vi starter med kun at hente brugere der har en afdeling (for ellers er der ingen lokation)
        var filter = Builders<Bruger>.Filter.Ne(b => b.Afdeling, null); 
        var brugere = await BrugerCollection.Find(filter).ToListAsync();

        // Nu filtrerer vi, så vi kun får én af hver unikke lokation
        return brugere
            .Where(b => b.Afdeling != null)
            .Select(b => b.Afdeling!)
            .GroupBy(l => l.LokationId)
            .Select(g => g.First())
            .ToList();
    }
    
    // Henter elevplan ud fra brugerens brugerId
    public async Task<Elevplan?> HentElevplanForBruger(int brugerId, int forespoergerId)
    {
        // Finder eleven ud fra brugerId (den elev, hvis plan vi ønsker at hente)
        var elev = await BrugerCollection.Find(b => b.BrugerId == brugerId).FirstOrDefaultAsync();

        // Finder den bruger, der anmoder om adgang (kan være elev, kok, HR, etc.)
        var forespoerger = await BrugerCollection.Find(b => b.BrugerId == forespoergerId).FirstOrDefaultAsync();

        // Hvis enten elev eller forespørger ikke findes, returneres null
        if (elev == null || forespoerger == null)
            return null;

        // Hvis forespørgeren er elev og prøver at tilgå en anden elevs plan
        if (forespoerger.Rolle == "Elev" && forespoerger.BrugerId != elev.BrugerId)
        {
            // ... og de ikke er fra samme lokation, så næg adgang
            if (forespoerger.Afdeling?.LokationNavn != elev.Afdeling?.LokationNavn)
                return null;
        }

        // Hvis adgang er tilladt, returner elevens elevplan
        return elev.MinElevplan;
    }
    
    public async Task<List<Bruger>> HentFiltreredeElever(string soegeord, string lokation, string kursus, string erhverv, int? deadline, string rolle, string? status, string? brugerLokation)
    {
        var filterBuilder = Builders<Bruger>.Filter;
        var filter = filterBuilder.Eq(b => b.Rolle, "Elev"); // Vis kun elever

        // Begræns til brugerens lokation, medmindre det er HR/Admin
        if (rolle != "HR" && rolle != "Admin" && !string.IsNullOrEmpty(brugerLokation))
        {
            filter &= filterBuilder.Eq(b => b.Afdeling.LokationNavn, brugerLokation);
        }

        // Almindelige filtre
        if (!string.IsNullOrWhiteSpace(lokation))
            filter &= filterBuilder.Eq(b => b.Afdeling.LokationNavn, lokation);

        if (!string.IsNullOrWhiteSpace(erhverv))
            filter &= filterBuilder.Eq(b => b.Erhverv, erhverv);

        if (!string.IsNullOrWhiteSpace(soegeord))
            filter &= filterBuilder.Regex(b => b.Navn, new MongoDB.Bson.BsonRegularExpression(soegeord, "i"));
        
        // Hvis vi filtrerer på overskredet deadline, vis kun elever hvor mindst ét delmål har deadline overskredet (før dags dato) OG som ikke er gennemført (Status == false)
        if (deadline.HasValue && deadline.Value == 0)
        {
            filter &= filterBuilder.Where(b =>
                b.MinElevplan.ListPerioder
                    .SelectMany(p => p.ListMaal)
                    .SelectMany(m => m.ListDelmaal)
                    .Any(d =>
                        d.Deadline != null &&
                        d.Deadline < DateOnly.FromDateTime(DateTime.Today) &&
                        d.Status == false));
        }
        
        if (!string.IsNullOrWhiteSpace(status) && !string.IsNullOrWhiteSpace(kursus))
        {
            // Hvis både status og kursus er valgt, så filtrér på:
            // - delmål hvor titlen matcher kurset
            // - og hvor status matcher "gennemført" (true) eller "ikke gennemført" (false)
            bool ønsketStatus = status == "gennemført";

            filter &= filterBuilder.Where(b =>
                b.MinElevplan.ListPerioder
                    .SelectMany(p => p.ListMaal)
                    .SelectMany(m => m.ListDelmaal)
                    .Any(d => d.Titel == kursus && d.Status == ønsketStatus));
        }
        else if (!string.IsNullOrWhiteSpace(kursus))
        {
            // Hvis kun kursus er valgt, så filtrér på om et delmål har denne titel
            filter &= filterBuilder.Where(b =>
                b.MinElevplan.ListPerioder
                    .SelectMany(p => p.ListMaal)
                    .SelectMany(m => m.ListDelmaal)
                    .Any(d => d.Titel == kursus));
        }
        else if (!string.IsNullOrWhiteSpace(status))
        {
            // Hvis kun status er valgt, så filtrér på om nogen delmål matcher status
            bool ønsketStatus = status == "gennemført";

            filter &= filterBuilder.Where(b =>
                b.MinElevplan.ListPerioder
                    .SelectMany(p => p.ListMaal)
                    .SelectMany(m => m.ListDelmaal)
                    .Any(d => d.Status == ønsketStatus));
        }
        
        return await BrugerCollection.Find(filter).ToListAsync();
    }

    public async Task<List<string>> HentErhverv()
    {
        var filter = Builders<Bruger>.Filter.Ne(b => b.Erhverv, null);
        var brugere = await BrugerCollection.Find(filter).ToListAsync();

        return brugere
            .Where(b => !string.IsNullOrWhiteSpace(b.Erhverv))
            .Select(b => b.Erhverv!)
            .Distinct()
            .ToList();
    }
    
    public async Task<List<string>> HentKurser()
    {
        var filter = Builders<Bruger>.Filter.Eq(b => b.Rolle, "Elev");
        var brugere = await BrugerCollection.Find(filter).ToListAsync();

        return brugere
            .SelectMany(b => b.MinElevplan?.ListPerioder ?? new List<Praktikperiode>())
            .SelectMany(p => p.ListMaal)
            .SelectMany(m => m.ListDelmaal)
            .Where(d => d.DelmaalType == "Kursus")
            .Select(d => d.Titel ?? "")
            .Distinct()
            .ToList();
    }
    public async Task OpdaterBillede(int brugerId, string sti)
    {
        var filter = Builders<Bruger>.Filter.Eq(b => b.BrugerId, brugerId);
        var update = Builders<Bruger>.Update.Set(b => b.Billede, sti);
        await BrugerCollection.UpdateOneAsync(filter, update);
    }

}
