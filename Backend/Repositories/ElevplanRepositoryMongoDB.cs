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
        var filter = Builders<Bruger>.Filter.Eq(b => b.MinElevplan.ElevplanId, elevplanId);
        var bruger = await BrugerCollection.Find(filter).FirstOrDefaultAsync();
        
        if (bruger == null || bruger.MinElevplan == null)
            throw new Exception($"Ingen elevplan med ID {elevplanId} fundet.");
        
        //finder det rigtige delmål, ved at matche delmålId fra det valgte delmål i frontend med det i databasen
        var delmaal = bruger.MinElevplan.ListPerioder
            .SelectMany(p => p.ListMaal)
            .SelectMany(m => m.ListDelmaal)
            .FirstOrDefault(d => d.DelmaalId == delmaalId);

        if (delmaal == null)
            throw new Exception($"Delmål med ID {delmaalId} blev ikke fundet.");
        
        //Tilføjer kommentaren til listen af kommentarer for delmålet
        delmaal.Kommentarer.Add(kommentar);

        //Opdaterer brugeren - dette gøres da kommentarer er dybt embedded og derfor er svære at tilgå
        await BrugerCollection.ReplaceOneAsync(b => b.BrugerId == bruger.BrugerId, bruger);
    }


    
}
