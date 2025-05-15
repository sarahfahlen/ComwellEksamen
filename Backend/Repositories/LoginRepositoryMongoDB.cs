using Backend.Repositories.Interface;
using MongoDB.Driver;
using Shared;

namespace Backend.Repositories;

/// <summary>
/// Denne klasse er ansvarlig for alt databasearbejde relateret til login og brugerhåndtering –
/// fx loginvalidering, hentning og opdatering af brugere. Den bruger MongoDB som database.
/// </summary>
public class LoginRepositoryMongoDB : ILoginRepository
{
    private IMongoClient client; // Forbindelse til MongoDB
    private IMongoCollection<Bruger> LoginCollection; // Selve collection'en vi arbejder med (Brugere)

    public LoginRepositoryMongoDB()
    {
        // Atlas-versionen (cloud-hostet MongoDB). Her bruges brugernavn + adgangskode.
        var password = "Comwell";
        var mongoUri =
            $"mongodb+srv://Comwell:{password}@comwell.mils9ta.mongodb.net/?retryWrites=true&w=majority&appName=Comwell";
        
        // var mongoUri = "mongodb://localhost:27017/";

        try
        {
            // Opretter klienten og forsøger at forbinde til databasen
            client = new MongoClient(mongoUri);
        }
        catch (Exception e)
        {
            Console.WriteLine("Der opstod en fejl ved forbindelse til databasen. " +
                              "Tjek at brugernavn, adgangskode og IP-whitelist er korrekt. " +
                              $"Fejlbesked: {e.Message}");
            throw; // stopper programmet – der er ingen grund til at køre videre uden database
        }

        // Navnet på databasen og samlingen (collection) i MongoDB vi arbejder med
        var dbName = "Comwell";
        var collectionName = "Brugere";

        // Her "åbner" vi forbindelsen til selve samlingen i databasen
        LoginCollection = client.GetDatabase(dbName)
            .GetCollection<Bruger>(collectionName);
    }


    // Henter alle brugere i databasen uden filter.
    // Dette bruges fx til når vi skal generer et id til en bruger.
   
    public Bruger[] HentAlleBrugere()
    {
        var nofilter = Builders<Bruger>.Filter.Empty; // tomt filter = hent alt
        return LoginCollection.Find(nofilter).ToList().ToArray(); // konverterer listen til array
    }

 
    // Kaldes når en bruger forsøger at logge ind. 
    // Finder brugeren i databasen ud fra email og adgangskode.
    // Bruges af LoginController → som bliver kaldt af frontendens LoginService.

    public async Task<Bruger?> Validering(string email, string adgangskode)
    {
        var bruger = await LoginCollection
            .Find(b => b.Email == email && b.Adgangskode == adgangskode)
            .FirstOrDefaultAsync(); // finder den første bruger der matcher
        return bruger; // returnerer null hvis ingen findes
    }

  
    // Opdaterer en eksisterende bruger i databasen.
    // Bruges af LoginController når en bruger ændrer sin profil eller adgangskode via frontend.
  
    public async Task<bool> OpdaterBrugerAsync(int id, Bruger bruger)
    {
        var filter = Builders<Bruger>.Filter.Eq(b => b.BrugerId, id); // find bruger med matching ID

        var eksisterende = await HentBrugerViaIdAsync(id); // tjek om brugeren eksisterer
        if (eksisterende == null)
            return false;

        //  Hvis der ikke sendes en ny adgangskode med fra frontend, så behold den gamle
        if (string.IsNullOrWhiteSpace(bruger.Adgangskode))
            bruger.Adgangskode = eksisterende.Adgangskode;

        // Erstat hele brugerens dokument i databasen
        var result = await LoginCollection.ReplaceOneAsync(filter, bruger);

        // Bekræft at opdateringen gik igennem (acknowledged) og at der faktisk blev ændret noget (ModifiedCount > 0)
        return result.IsAcknowledged && result.ModifiedCount > 0;
    }


    // Henter en enkelt bruger ud fra brugerens ID.
    // Bruges fx ved login eller når en bruger skal vises eller opdateres.
    public async Task<Bruger?> HentBrugerViaIdAsync(int id)
    {
        var filter = Builders<Bruger>.Filter.Eq(b => b.BrugerId, id); // filter der matcher på ID
        return await LoginCollection.Find(filter).FirstOrDefaultAsync(); // returnér første (eller null)
    }
}
