using Backend.Repositories.Interface;
using Microsoft.AspNetCore.Mvc;
using Shared;

namespace Backend.Controllers;

[ApiController]
[Route("api/elevplan")]
public class ElevplanController : ControllerBase
{
    private readonly IElevplanRepository elevplanRepo;
    private readonly IBrugereRepository brugereRepo;

    public ElevplanController(IElevplanRepository elevplanRepo)
    {
        this.elevplanRepo = elevplanRepo;
    }  
    
    //Henter skabelonen fra repository - returnerer NotFound hvis fejl, ellers returnerer den skabelonen
    //ActionResult bruges til enten at returnere Elevplan eller en fejlbesked 
    [HttpGet("skabelon/{navn}")]
    public async Task<ActionResult<Elevplan>> HentSkabelon(string navn)
    {
        var skabelon = await elevplanRepo.HentSkabelon(navn);
        if (skabelon == null)
            return NotFound($"Intet skabelon med navn '{navn}' fundet.");

        return Ok(skabelon);
    }
    
    //Henter kommentaren baseret på elevplan, delmål og rolle - ved at kalde på repo
    [HttpGet("kommentar/{elevplanId:int}/{delmaalId:int}/{brugerRolle}")]
    public async Task<ActionResult<Kommentar>> GetKommentar(int elevplanId, int delmaalId, string brugerRolle)
    {
        var kommentar = await elevplanRepo.GetKommentarAsync(elevplanId, delmaalId, brugerRolle);
        
        //fejlhåndtering
        if (kommentar == null)
            return NotFound("Ingen kommentar fundet for det givne delmål og rolle.");

        return Ok(kommentar);
    }

    
    //Tilføjer en ny kommentar ved at kalde repo funktion
    [HttpPost("kommentar/{elevplanId:int}/{delmaalId:int}")]
    public async Task<IActionResult> TilfoejKommentar(int elevplanId, int delmaalId, [FromBody] Kommentar kommentar)
    {
        try
        {
            await elevplanRepo.TilfoejKommentar(elevplanId, delmaalId, kommentar);
            return Ok("Kommentar tilføjet");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TilfoejKommentar] FEJL: {ex.Message}");
            return BadRequest(ex.Message);
        }
    }
    
    //Opdaterer kommentaren via put, og kalder funktion fra repo
    [HttpPut("kommentar/{elevplanId:int}/{delmaalId:int}/{kommentarId:int}")]
    public async Task<IActionResult> RedigerKommentar(int elevplanId, int delmaalId, int kommentarId, [FromBody] string nyTekst)
    {
        try
        {
            await elevplanRepo.RedigerKommentarAsync(elevplanId, delmaalId, kommentarId, nyTekst);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest($"Fejl ved redigering af kommentar: {ex.Message}");
        }
    }


}