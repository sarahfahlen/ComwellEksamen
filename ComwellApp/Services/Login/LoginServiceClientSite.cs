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
        foreach (var u in brugere)
        {
            if (email == u.Email && adgangskode == u.Adgangskode)
            {
                u.Adgangskode = "validated";
                await _localStorage.SetItemAsync("bruger", u);
                return true;
            }
        }

        return false;
    }

    public async Task<Bruger[]> GetAll()
    {
        var brugere = await _brugereService.HentAlle();
        return brugere.ToArray();
    }

}