using System.Net.Http.Json;
using Shared;

namespace ComwellApp.Services.Lokation;

public class LokationServiceServer : ILokationService
{
    private readonly HttpClient _http;

    public LokationServiceServer(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<Shared.Lokation>> HentKoekkenLokationer()
    {
        return await _http.GetFromJsonAsync<List<Shared.Lokation>>("api/lokationer/koekken") ?? new();
    }

    public async Task<List<Shared.Lokation>> HentSkoleLokationer()
    {
        return await _http.GetFromJsonAsync<List<Shared.Lokation>>("api/lokationer/skole") ?? new();
    }

}