using Shared;
namespace Backend.Repositories.Interface;

public interface IElevplanRepository
{
    //Bruges til at hente den skabelon der skal oprettes, hentes baseret på skabelonNavn (kok i dette system)
    Task<Elevplan?> HentSkabelon(string skabelonNavn);
    
    //Bruges til at tilføje en kommentar, baseret på elevplan, delmål og selve kommentaren
    Task TilfoejKommentar(int elevplanId, int delmaalId, Kommentar kommentar);
    
    //Bruges til at hente kommentarer, baseret på elevplan, delmål og rollen den er oprettet med
    Task<Kommentar?> GetKommentarAsync(int elevplanId, int delmaalId, string brugerRolle);
    
    //Bruges til at redigere kommentarer, baseret på elevplan, delmål, kommentar og den nye tekst
    Task RedigerKommentarAsync (int elevplanId, int delmaalId, int kommentarId, string nyTekst);


    
}