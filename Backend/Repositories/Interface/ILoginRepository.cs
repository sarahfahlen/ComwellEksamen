using Shared;

namespace Backend.Repositories.Interface;

public interface ILoginRepository
{
    Bruger[] HentAlleBrugere();
    Task<Bruger?> Validering(string email, string adgangskode);
    Task<bool> OpdaterBrugerAsync(int id, Bruger bruger);
    public class LoginRequest
    {
        public string Email { get; set; }
        public string Adgangskode { get; set; }
    }
}