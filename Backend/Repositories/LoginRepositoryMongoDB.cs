using Backend.Repositories.Interface;
using MongoDB.Driver;
using Shared;

namespace Backend.Repositories;

public class LoginRepositoryMongoDB : ILoginRepository
{
    private IMongoClient client;
    private IMongoCollection<Bruger> LoginCollection;


    public LoginRepositoryMongoDB()
    {
        // atlas database
        var password = "Comwell";
        var mongoUri =
            $"mongodb+srv://Comwell:{password}@comwell.mils9ta.mongodb.net/?retryWrites=true&w=majority&appName=Comwell";

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
        var collectionName = "Brugere";

        LoginCollection = client.GetDatabase(dbName)
            .GetCollection<Bruger>(collectionName);
    }

    public Bruger[] HentAlleBrugere()
    {
        var nofilter = Builders<Bruger>.Filter.Empty;
        return LoginCollection.Find(nofilter).ToList().ToArray();
    }


    public async Task<Bruger?> Validering(string email, string adgangskode)
    {
        var bruger = await LoginCollection.Find(b => b.Email == email && b.Adgangskode == adgangskode)
            .FirstOrDefaultAsync();
        return bruger;
    }
    public async Task<bool> OpdaterBrugerAsync(int id, Bruger bruger)
    {
        var filter = Builders<Bruger>.Filter.Eq(b => b.BrugerId, id);
        var eksisterende = await HentBrugerViaIdAsync(id);

        if (eksisterende == null)
            return false;

        // 🛡 Bevar adgangskoden hvis feltet er tomt
        if (string.IsNullOrWhiteSpace(bruger.Adgangskode))
            bruger.Adgangskode = eksisterende.Adgangskode;

        var result = await LoginCollection.ReplaceOneAsync(filter, bruger);
        return result.IsAcknowledged && result.ModifiedCount > 0;
    }

    public async Task<Bruger?> HentBrugerViaIdAsync(int id)
    {
        var filter = Builders<Bruger>.Filter.Eq(b => b.BrugerId, id);
        return await LoginCollection.Find(filter).FirstOrDefaultAsync();
    }

}