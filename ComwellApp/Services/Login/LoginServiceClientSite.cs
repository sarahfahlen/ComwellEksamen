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

    public static Bruger Kasper = new Bruger
        { BrugerId = 1, Navn = "Kasper", Adgangskode = "1234", Email = "kasper@mail.com", Telefon = 76546789, Rolle = "Køkkenchef"};

    public static Bruger Emil = new Bruger
        { BrugerId = 2, Navn = "Emil", Adgangskode = "1234", Email = "emil@mail.com", Telefon = 87907652, Rolle = "Elev" };

    public static Bruger Frank = new Bruger
        { BrugerId = 3, Navn = "Frank", Adgangskode = "1234", Email = "frank@mail.com", Telefon = 64572358, Rolle = "FaglærtKok" };

    public static List<Bruger> users = new List<Bruger> { Kasper, Emil, Frank };
    
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
        foreach (Bruger u in users)

            if (email.Equals(u.Email) && adgangskode.Equals(u.Adgangskode))
                return u;

        return null;
    }

    public async Task<Bruger[]> GetAll()
    {
        return users.ToArray();
    }
}