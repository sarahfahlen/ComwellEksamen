using Shared;
namespace ComwellApp.Services.Login;

public interface ILoginService
{
    //Henter bruger fra LocalStorage hvis der er en logget ind
    Task<Bruger> GetUserLoggedIn();
    
    //Logger brugeren ind og opretter bruger i LocalStorage- hvis email og adganskode bliver valideret
    Task<bool> Login(string Email, string Adgangskode);
    
    Task<Bruger[]> GetAll();
    
    Task GemElevILocalStorage(Bruger elev);


}   