using Backend.Controllers;
using Backend.Repositories.Interface;
using MongoDB.Driver;
using Shared;

namespace Backend.Repositories;

public class BrugereRepositoryMongoDB : IBrugereRepository
{
    
        private IMongoClient client;
        private IMongoCollection<Bruger> BrugerCollection;

        public BrugereRepositoryMongoDB()
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
            var collectionName = "Brugere";

            BrugerCollection = client.GetDatabase(dbName)
                .GetCollection<Bruger>(collectionName);
            
        }
        
        //Tilføjer den nye elev til MongoDB
        public async Task TilfoejElev(Bruger nyBruger)
        {
            // Hvis BrugerId er 0 (ikke sat fra frontend), generer en
            if (nyBruger.BrugerId == 0)
            {
                var eksisterendeBrugerIds = await BrugerCollection
                    .Find(Builders<Bruger>.Filter.Empty)
                    .Project(b => b.BrugerId)
                    .ToListAsync();

                int nytBrugerId = (eksisterendeBrugerIds.Any()) 
                    ? eksisterendeBrugerIds.Max() + 1 
                    : 1;

                nyBruger.BrugerId = nytBrugerId;

                Console.WriteLine($"[Repo] Ny BrugerId genereret: {nytBrugerId}");
            }


            await BrugerCollection.InsertOneAsync(nyBruger);
        }

        public async Task<List<Bruger>> HentAlle()
        {
            var filter = Builders<Bruger>.Filter.Empty;
            return await BrugerCollection.Find(filter).ToListAsync();
        }
        public async Task<List<Bruger>> HentAlleElever()
        {
            var filter = Builders<Bruger>.Filter.Eq(b => b.Rolle, "Elev");
            return await BrugerCollection.Find(filter).ToListAsync();
        }

        public async Task<List<Bruger>> HentAlleKøkkenchefer()
        {
            var filter = Builders<Bruger>.Filter.Eq(b => b.Rolle, "Køkkenchef");
            return await BrugerCollection.Find(filter).ToListAsync();
        }
        public async Task<List<Lokation>> HentAlleLokationer()
        {
            var filter = Builders<Bruger>.Filter.Ne(b => b.Afdeling, null);
            var brugere = await BrugerCollection.Find(filter).ToListAsync();

            return brugere
                .Where(b => b.Afdeling != null)
                .Select(b => b.Afdeling!)
                .GroupBy(l => l.LokationId)
                .Select(g => g.First())
                .ToList();
        }

}