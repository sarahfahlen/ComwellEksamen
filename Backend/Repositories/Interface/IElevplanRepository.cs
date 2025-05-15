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

    //Bruges til at opdatere status for et delmål, baseret på elevplan og det pågældende delmål
    Task OpdaterStatusAsync(int elevplanId, Delmaal delmaal);

    Task TilfoejDelmaal(int elevplanId, int maalId, Delmaal nytDelmaal);
    Task<Elevplan?> HentElevplanMedMaal(int elevplanId, int periodeIndex);


    // Brugt til at hente en elevplans mål med filtre og søgning fra frontend
    Task<List<Maal>> HentFiltreredeMaal(
        int brugerId,                     // Brugerens ID – bruges til at finde elevplanen
        int periodeIndex,                 // Index for den praktikperiode der ønskes
        string? valgtMaalNavn,            // Mål-navn filter – fx "Intro"
        string? valgtDelmaalType,         // Delmålstype filter – fx "Samtale"
        string? soegeord,                  // Søgeord – bruges til fritekstsøgning i delmålstitel
        bool? filterStatus                // Filter på gennemført-status: true/false/null
    );
}