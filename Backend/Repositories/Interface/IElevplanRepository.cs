using Shared;
namespace Backend.Repositories.Interface;

public interface IElevplanRepository
{
    //Bruges til at hente den skabelon der skal oprettes, hentes baseret på skabelonNavn (kok i dette system)
    Task<Elevplan?> HentSkabelon(string skabelonNavn);
    
}