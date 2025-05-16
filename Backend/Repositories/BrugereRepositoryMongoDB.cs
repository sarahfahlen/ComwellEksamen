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
    public async Task<Elevplan?> HentElevplanForBruger(int brugerId)
    {
        var bruger = await BrugerCollection.Find(b => b.BrugerId == brugerId).FirstOrDefaultAsync();
        return bruger?.MinElevplan;
    }
    
    public async Task<List<Bruger>> HentFiltreredeElever(string? navn, string? lokation, string? kursus, string? erhverv, int? deadlineDage)
    {
        var builder = Builders<Bruger>.Filter;
        var filters = new List<FilterDefinition<Bruger>>();

        if (!string.IsNullOrWhiteSpace(navn))
            filters.Add(builder.Regex(b => b.Navn, new MongoDB.Bson.BsonRegularExpression(navn, "i")));

        if (!string.IsNullOrWhiteSpace(lokation))
            filters.Add(builder.Eq(b => b.Afdeling.LokationNavn, lokation));

        if (!string.IsNullOrWhiteSpace(kursus))
            filters.Add(builder.ElemMatch(b => b.MinElevplan.ListPerioder, p => p.ListMaal.Any(m => m.MaalNavn == kursus)));

        if (!string.IsNullOrWhiteSpace(erhverv))
            filters.Add(builder.ElemMatch(b => b.MinElevplan.ListPerioder, p => p.ListMaal.Any(m => m.ListDelmaal.Any(d => d.Titel.Contains(erhverv)))));

        if (deadlineDage.HasValue)
            filters.Add(builder.ElemMatch(b => b.MinElevplan.ListPerioder,
                p => p.ListMaal.Any(m =>
                    m.ListDelmaal.Any(d => d.Deadline.HasValue && (d.Deadline.Value.DayNumber - DateOnly.FromDateTime(DateTime.Today).DayNumber) <= deadlineDage.Value)
                )));

        var combinedFilter = filters.Any() ? builder.And(filters) : builder.Empty;

        return await BrugerCollection.Find(combinedFilter).ToListAsync();
    }


}
