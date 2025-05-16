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
        try
        {
            // Vi beder repo’en hente alle brugere (uanset rolle)
            var brugere = await _repo.HentAlle();

            // Hvis der mod forventning ikke kommer noget tilbage, sender vi 404
            if (brugere == null || !brugere.Any())
                return NotFound("Ingen brugere blev fundet i databasen.");

            // Vi pakker listen ind i et 200 OK-svar og returnerer den
            return Ok(brugere);
        }
        catch (Exception ex)
        {
            // Hvis noget går galt, sender vi en 500-fejl og logger fejlen
            return StatusCode(500, $"Der opstod en fejl ved hentning af brugere: {ex.Message}");
        }
    }


    // GET: /api/brugere/elever
    // Denne endpoint henter kun dem, der har rollen "Elev" – bruges f.eks. i dashboard 
    [HttpGet("elever")]
    public async Task<ActionResult<List<Bruger>>> HentAlleElever()
    {
        try
        {
            var elever = await _repo.HentAlleElever();

            if (elever == null || !elever.Any())
                return NotFound("Der er ingen elever registreret.");

            return Ok(elever);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Fejl under hentning af elever: {ex.Message}");
        }
    }


    // GET: /api/brugere/køkkencheferlokation/{lokationId}
    // Returnerer køkkenchefer knyttet til en specifik lokation
    [HttpGet("køkkencheferlokation/{lokationId:int}")]
    public async Task<ActionResult<List<Bruger>>> HentKøkkencheferTilLokation(int lokationId)
    {
        try
        {
            if (lokationId <= 0)
                return BadRequest("Ugyldigt lokations-ID – det skal være større end 0.");

            var brugere = await _repo.HentAlleKøkkenchefer(); // Repo returnerer allerede KUN køkkenchefer

            // Filtrerer dem, der arbejder på den valgte lokation
            var filtrerede = brugere
                .Where(b => b.Afdeling?.LokationId == lokationId)
                .ToList();

            if (!filtrerede.Any())
                return NotFound($"Ingen køkkenchefer fundet for lokation med ID {lokationId}.");

            return Ok(filtrerede);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Fejl under hentning af køkkenchefer: {ex.Message}");
        }
    }


    // GET: /api/brugere/lokationer
    // Denne metode bruges til at hente en liste af alle unikke lokationer, baseret på brugernes tilknytning
    [HttpGet("lokationer")]
    public async Task<ActionResult<List<Lokation>>> HentAlleLokationer()
    {
        try
        {
            // Vi bruger repo til at hente alle lokationer (unik baseret på LokationId)
            var lokationer = await _repo.HentAlleLokationer();

            if (lokationer == null || !lokationer.Any())
                return NotFound("Der blev ikke fundet nogen lokationer.");

            return Ok(lokationer);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Fejl under hentning af lokationer: {ex.Message}");
        }
    }
    
    // Henter brugerens elevplan ud fra brugerId
    [HttpGet("{brugerId}/elevplan")]
    public async Task<ActionResult<Elevplan>> HentElevplanForBruger(int brugerId)
    {
        var plan = await _repo.HentElevplanForBruger(brugerId);
        if (plan == null)
            return NotFound($"Ingen elevplan fundet for brugerId {brugerId}");

        return Ok(plan);
    }
    
    [HttpGet("filtrerede")]
    public async Task<ActionResult<List<Bruger>>> HentFiltreredeElever(
        [FromQuery] string? navn,
        [FromQuery] string? lokation,
        [FromQuery] string? kursus,
        [FromQuery] string? erhverv,
        [FromQuery] int? deadlineDage)
    {
        try
        {
            var resultater = await _repo.HentFiltreredeElever(navn, lokation, kursus, erhverv, deadlineDage);
            return Ok(resultater);
        }
        catch (Exception ex)
        {
            return BadRequest($"Fejl under filtrering: {ex.Message}");
        }
    }



}
