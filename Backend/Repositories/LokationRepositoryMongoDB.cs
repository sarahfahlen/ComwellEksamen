using Backend.Repositories.Interface;
using MongoDB.Driver;
using Shared;

namespace Backend.Repositories;

public class LokationRepositoryMongoDB : ILokationRepository
{
    private readonly IMongoCollection<Lokation> LokationCollection;

    public LokationRepositoryMongoDB()
    {
        var password = "Comwell";
        var mongoUri = $"mongodb+srv://Comwell:{password}@comwell.mils9ta.mongodb.net/?retryWrites=true&w=majority&appName=Comwell";

        var client = new MongoClient(mongoUri);
        var db = client.GetDatabase("Comwell");
        LokationCollection = db.GetCollection<Lokation>("Lokation");
    }

    public async Task<List<Lokation>> HentLokationerAfType(string type)
    {
        var filter = Builders<Lokation>.Filter.Eq(l => l.LokationType, type);
        return await LokationCollection.Find(filter).ToListAsync();
    }
}
