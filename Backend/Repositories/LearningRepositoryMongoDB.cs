using Backend.Controllers;
using Backend.Repositories.Interface;
using MongoDB.Driver;
using Shared;

namespace Backend.Repositories;

public class LearningRepositoryMongoDB : ILearningRepository
{
    private IMongoClient client; // Klienten der opretter forbindelse til MongoDB-serveren (Atlas eller lokal)
    private IMongoCollection<Fagomraade> LearningCollection; // Collection = tabel i MongoDB – her arbejder vi med fagområder


    //Konstruktøren kører når objektet bliver oprettet – her opretter vi databasen og samlingen.
    public LearningRepositoryMongoDB()
    {
        // Atlas-forbindelse – adgang via brugernavn og kode til cloud MongoDB (Atlas)
        var password = "Comwell";
        var mongoUri = $"mongodb+srv://Comwell:{password}@comwell.mils9ta.mongodb.net/?retryWrites=true&w=majority&appName=Comwell";

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
        var collectionName = "Learning";

        // Nu henter vi selve collectionen vi skal arbejde med – altså listen over fagområder
        LearningCollection = client.GetDatabase(dbName)
            .GetCollection<Fagomraade>(collectionName);
    }

    public async Task<List<Fagomraade>> HentAlleFagomraader()
    {
        // Først hent hele listen fra MongoDB
        var result = await LearningCollection.Find(_ => true).ToListAsync();
        foreach (var fag in result)
        {
            foreach (var underemne in fag.ListUnderemne)
            {
                foreach (var element in underemne.ListElement)
                {
                    Console.WriteLine("TYPE: " + element.GetType().Name);
                }
            }
        }
        return result;
    }


}