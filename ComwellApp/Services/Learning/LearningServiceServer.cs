using System.Net.Http.Json;
using Shared;

namespace ComwellApp.Services.Learning;

public class LearningServiceServer : ILearningService
{
    private readonly HttpClient http;
    private readonly IdGeneratorService _idGenerator;

    public LearningServiceServer(HttpClient http, IdGeneratorService idGenerator)
    {
        this.http = http;
        _idGenerator = idGenerator;
    }
    
    public async Task<List<Fagomraade>> HentAlleFagomraader()
    {
        var result = await http.GetFromJsonAsync<List<Fagomraade>>("api/learning");
        return result ?? new List<Fagomraade>();
    }

}