using Blazored.LocalStorage;
using System.Net.Http.Json;
using Shared;

namespace ComwellApp.Services.Login;

public class LoginServiceServer : ILoginService
{
    private readonly string serverUrl = "http://localhost:5237";
    private readonly HttpClient client;
    private readonly ILocalStorageService localStorage;

    public LoginServiceServer(HttpClient client, ILocalStorageService localStorage)
    {
        this.client = client;
        this.localStorage = localStorage;
    }
    public async Task<Bruger?> GetUserLoggedIn()
    {
        return await localStorage.GetItemAsync<Bruger>("bruger");
    }

    public async Task<bool> Login(string Email, string Adgangskode)
    {
        var body = new { Email = Email, Adgangskode = Adgangskode };

        try
        {
            var response = await client.PostAsJsonAsync($"{serverUrl}/api/users/login", body);

            if (!response.IsSuccessStatusCode)
                return false;

            var bruger = await response.Content.ReadFromJsonAsync<Bruger>();

            if (bruger != null)
            {
                bruger.Adgangskode = null; 
                await localStorage.SetItemAsync("bruger", bruger);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Login fejlede: {ex.Message}");
            return false;
        }
    }

    public async Task<Bruger[]> GetAll()
    {
        return await client.GetFromJsonAsync<Bruger[]>($"{serverUrl}/api/users");
    }

    public Task GemElevILocalStorage(Bruger elev)
    {
        throw new NotImplementedException();
    }

    public Task<List<Bruger>> HentAlleGemteEleverFraLocalStorage()
    {
        throw new NotImplementedException();
    }
    public async Task OpdaterBruger(Bruger bruger)
    {
        var response = await client.PutAsJsonAsync($"{serverUrl}/api/users/{bruger.BrugerId}", bruger);

        if (!response.IsSuccessStatusCode)
        {
            var fejl = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Opdatering fejlede: {fejl}");
        }
    }
    public async Task<bool> SkiftAdgangskode(int brugerId, string nuværendeKode, string nyKode)
    {
        var requestBody = new
        {
            NuværendeKode = nuværendeKode,
            NyKode = nyKode
        };

        var response = await client.PutAsJsonAsync($"{serverUrl}/api/users/{brugerId}/skiftkode", requestBody);

        if (!response.IsSuccessStatusCode)
        {
            var fejl = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Fejl ved adgangskodeskift: {fejl}");
            return false;
        }

        return true;
    }

}