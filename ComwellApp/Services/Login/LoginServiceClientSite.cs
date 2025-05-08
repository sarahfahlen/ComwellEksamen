using Blazored.LocalStorage;
using Shared;

namespace ComwellApp.Services.Login;

public class LoginServiceClientSite : ILoginService
{
    private ILocalStorageService localStorage { get; set; }

    public LoginServiceClientSite(ILocalStorageService ls)
    {
        localStorage = ls;
    }

    public static List<Bruger> brugere = Brugere.BrugereServiceMock.brugere;
    
    public async Task<Bruger?> GetUserLoggedIn()
    {
        Bruger? res = await localStorage.GetItemAsync<Bruger>("bruger");
        return res;
    }
    
    public async Task<bool> Login(string email, string adgangskode)
    {
        Bruger? u = await Validate(email, adgangskode);
        if (u != null)
        {
            u.Adgangskode = "validated";
            await localStorage.SetItemAsync("bruger", u);
            return true;
        }

        return false;
    }
    protected virtual async Task<Bruger?> Validate(string email, string adgangskode)
    {
        foreach (Bruger u in brugere)

            if (email.Equals(u.Email) && adgangskode.Equals(u.Adgangskode))
                return u;

        return null;
    }

    public async Task<Bruger[]> GetAll()
    {
        return brugere.ToArray();
    }
}