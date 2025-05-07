using Shared;
namespace ComwellApp.Services.Login;

public interface ILoginService
{
    Task<Bruger> GetUserLoggedIn();
    
    Task<bool> Login(string Email, string Adgangskode);
    
    Task<Bruger[]> GetAll();
}