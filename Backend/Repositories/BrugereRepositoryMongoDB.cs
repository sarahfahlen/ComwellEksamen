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
}