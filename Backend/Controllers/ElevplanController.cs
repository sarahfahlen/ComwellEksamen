using Backend.Repositories.Interface;
using Microsoft.AspNetCore.Mvc;
using Shared;

namespace Backend.Controllers;

[ApiController]
[Route("api/elevplan")]
public class ElevplanController : ControllerBase
{
    private readonly IElevplanRepository elevplanRepo;

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
}