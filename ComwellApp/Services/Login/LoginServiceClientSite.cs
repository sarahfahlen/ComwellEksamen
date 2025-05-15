using Blazored.LocalStorage;
using ComwellApp.Services.Brugere;
using ComwellApp.Services.Login;
using Shared;

public class LoginServiceClientSite : ILoginService
{
    private readonly ILocalStorageService _localStorage;
    private readonly IBrugereService _brugereService;

    public LoginServiceClientSite(ILocalStorageService localStorage, IBrugereService brugereService)
    {
        _localStorage = localStorage;
        _brugereService = brugereService;
    }

    public async Task<Bruger?> GetUserLoggedIn()
    {
        return await _localStorage.GetItemAsync<Bruger>("bruger");
    }

    public async Task<bool> Login(string email, string adgangskode)
    {
        var brugere = await _brugereService.HentAlle();

        // Tjek mock-brugere
        foreach (var u in brugere)
        {
            if (email == u.Email && adgangskode == u.Adgangskode)
            {
                //husk at når det sættes op med database, så skal det ændres til at det ikke er loggeninuser men at den henter fra databasen og password bliver validated færst derefter
                await _localStorage.SetItemAsync("bruger", u);
                return true;
            }
        }

        // Tjek gemte elever
        var gemteElever = await HentEleverTilElevplanVisning();
        foreach (var elev in gemteElever)
        {
            if (email == elev.Email && adgangskode == elev.Adgangskode)
            {
                //husk at når det sættes op med database, så skal det ændres til at det ikke er loggeninuser men at den henter fra databasen og password bliver validated færst derefter
                await _localStorage.SetItemAsync("bruger", elev);
                return true;
            }
        }

        return false;
    }


    public async Task GemElevILocalStorage(Bruger elev)
    {
        var gemte = await _localStorage.GetItemAsync<List<Bruger>>("gemteElever") ?? new List<Bruger>();
        gemte.Add(elev);
        await _localStorage.SetItemAsync("gemteElever", gemte);
    }

    public async Task<List<Bruger>> HentEleverTilElevplanVisning()
    {
        return await _localStorage.GetItemAsync<List<Bruger>>("gemteElever") ?? new List<Bruger>();
    }


    public async Task<Bruger[]> GetAll()
    {
        var brugere = await _brugereService.HentAlle();
        return brugere.ToArray();
    }

    public async Task OpdaterBruger(Bruger bruger)
    {
        var gemte = await HentEleverTilElevplanVisning();
        var index = gemte.FindIndex(b => b.BrugerId == bruger.BrugerId);

        if (index != -1)
        {
            gemte[index] = bruger;
            await _localStorage.SetItemAsync("gemteElever", gemte);
            await _localStorage.SetItemAsync("bruger", bruger);
        }
    }

    public Task<bool> SkiftAdgangskode(int brugerId, string nuværendeKode, string nyKode)
    {
        throw new NotImplementedException();
    }
}