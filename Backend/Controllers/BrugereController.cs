using Backend.Repositories.Interface;
using Microsoft.AspNetCore.Mvc;
using Shared;

namespace Backend.Controllers;

// Her fortæller vi, at det her er en API-controller – det betyder, at den håndterer HTTP-kald fra frontend
[ApiController]
// Ruten starter med /api/brugere – det er den base-url der skal bruges for at ramme denne controller
[Route("api/brugere")]
public class BrugereController : ControllerBase
{
    // Repository er vores adgang til databasen – vi bruger interface så vi nemt kan skifte mellem f.eks. Mongo eller mock
    private readonly IBrugereRepository _repo;

    // Vi får repo'en ind via dependency injection, så den automatisk bliver sat når controlleren oprettes
    public BrugereController(IBrugereRepository repo)
    {
        _repo = repo;
    }
    
    // POST: /api/brugere
    // Denne metode bruges til at oprette en ny elev. Frontend sender en JSON-body med en Bruger, som vi modtager via [FromBody]
    [HttpPost]
    public async Task<IActionResult> TilfoejElev([FromBody] Bruger nyBruger)
    {
        // Hvis frontend har glemt at sende noget eller sender null, returnerer vi en 400 Bad Request
        if (nyBruger == null)
            return BadRequest("Bruger er null");

        // Ellers sender vi brugeren videre til repository, som står for at gemme i databasen
        await _repo.TilfoejElev(nyBruger);

        // Til sidst svarer vi tilbage med 200 OK og en besked
        return Ok("Bruger med elevplan oprettet");
    }

    // GET: /api/brugere
    [HttpGet]
    public async Task<ActionResult<List<Bruger>>> HentAlle()
    {
        // Vi beder repo’en hente alle brugere (uanset rolle)
        var brugere = await _repo.HentAlle();

        // Vi pakker listen ind i et 200 OK-svar og returnerer den
        return Ok(brugere);
    }

    // GET: /api/brugere/elever
    // Denne endpoint henter kun dem, der har rollen "Elev" – bruges f.eks. i dashboard 
    [HttpGet("elever")]
    public async Task<ActionResult<List<Bruger>>> HentAlleElever()
    {
        var elever = await _repo.HentAlleElever();
        return Ok(elever);
    }

    // GET: /api/brugere/køkkencheferlokation/id på lokation
    [HttpGet("køkkencheferlokation/{lokationId:int}")]
    public async Task<ActionResult<List<Bruger>>> HentKøkkencheferTilLokation(int lokationId)
    {
        var brugere = await _repo.HentAlleKøkkenchefer(); // Repo returnerer allerede KUN køkkenchefer

        // Filtrerer dem, der arbejder på den valgte lokation
        var filtrerede = brugere
            .Where(b => b.Afdeling?.LokationId == lokationId)
            .ToList();

        return Ok(filtrerede);
    }


    // GET: /api/brugere/lokationer
    // Denne metode bruges til at hente en liste af alle unikke lokationer, baseret på brugernes tilknytning
    [HttpGet("lokationer")]
    public async Task<ActionResult<List<Lokation>>> HentAlleLokationer()
    {
        // Vi bruger repo til at hente alle lokationer (unik baseret på LokationId)
        var lokationer = await _repo.HentAlleLokationer();

        return Ok(lokationer);
    }
}
