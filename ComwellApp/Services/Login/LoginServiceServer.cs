using Blazored.LocalStorage;
using System.Net.Http.Json;
using Shared;

namespace ComwellApp.Services.Login;

// Denne klasse er en implementation af ILoginService – og håndterer login, lokal brugerhentning, og kommunikation med backend API’et.
public class LoginServiceServer : ILoginService
{
    // URL til backend-API (kan evt. ændres til en config senere)
    private readonly string serverUrl = "http://localhost:5237";

    private readonly HttpClient client; // Brugt til HTTP-kald til backend
    private readonly ILocalStorageService localStorage; // Brugt til at gemme data lokalt i browseren (fx den loggede bruger)

    public LoginServiceServer(HttpClient client, ILocalStorageService localStorage)
    {
        this.client = client;
        this.localStorage = localStorage;
    }
    
    // Returnerer den bruger der er gemt lokalt i browseren (bruges mange steder i frontend til at kende "hvem er jeg?")
   
    public async Task<Bruger?> GetUserLoggedIn()
    {
        return await localStorage.GetItemAsync<Bruger>("bruger");
    }


    // Login-funktion: sender e-mail og adgangskode til backend for validering

    public async Task<bool> Login(string Email, string Adgangskode)
    {
        // Vi opretter en anonym body med e-mail og adgangskode (samme struktur som backend forventer)
        var body = new { Email = Email, Adgangskode = Adgangskode };

        try
        {
            // POST til backendens login-endpoint
            var response = await client.PostAsJsonAsync($"{serverUrl}/api/users/login", body);

            // Hvis login fejler (forkert kode, manglende bruger), så returnér false
            if (!response.IsSuccessStatusCode)
                return false;

            // Hvis login lykkes, henter vi brugerobjektet fra svaret
            var bruger = await response.Content.ReadFromJsonAsync<Bruger>();

            // Hvis der kommer noget retur – gem brugeren i LocalStorage
            if (bruger != null)
            {
                bruger.Adgangskode = null; // Vi fjerner adgangskoden, så den ikke gemmes i browseren (god praksis)
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


    // Henter alle brugere (typisk kun brugt af HR/Admin views til oversigter)

    public async Task<Bruger[]> GetAll()
    {
        return await client.GetFromJsonAsync<Bruger[]>($"{serverUrl}/api/users");
    }

 
    // Ikke implementeret – tidligere brugt i mock måske

    public Task GemElevILocalStorage(Bruger elev)
    {
        throw new NotImplementedException(); // Vi bruger ikke denne længere, da den blev brugt på mock
    }


    // Henter alle elever (typisk brugt til dropdowns, oversigter og elevplanvisning)

    public async Task<List<Bruger>> HentEleverTilElevplanVisning()
    {
        var elever = await client.GetFromJsonAsync<List<Bruger>>($"{serverUrl}/api/brugere/elever");
        return elever ?? new List<Bruger>();
    }

    
    // Opdaterer info om en bruger, fx ved redigering af profil
   
    public async Task OpdaterBruger(Bruger bruger)
    {
        var response = await client.PutAsJsonAsync($"{serverUrl}/api/users/{bruger._id}", bruger);

        if (!response.IsSuccessStatusCode)
        {
            var fejl = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Opdatering fejlede: {fejl}");
        }
    }
    
    // Skifter adgangskode – kræver at man oplyser nuværende kode som sikkerhed
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
