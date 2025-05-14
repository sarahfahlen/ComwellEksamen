using Backend.Repositories.Interface;
using Microsoft.AspNetCore.Mvc;
using Shared;

namespace Backend.Controllers;

[ApiController]
[Route("api/brugere")]
public class BrugereController : ControllerBase
{
    private readonly IBrugereRepository _repo;

    public BrugereController(IBrugereRepository repo)
    {
        _repo = repo;
    }
    
    //Metode der opretter ny elev via Post - denne bruges læses fra HTTP'ens body
    [HttpPost]
    public async Task<IActionResult> TilfoejElev([FromBody] Bruger nyBruger)
    {
        if (nyBruger == null)
            return BadRequest("Bruger er null");
        //Sender videre til repository
        await _repo.TilfoejElev(nyBruger);
        return Ok("Bruger med elevplan oprettet");
    }
    [HttpGet]
    public async Task<ActionResult<List<Bruger>>> HentAlle()
    {
        var brugere = await _repo.HentAlle(); // den metode skal du også have i dit repo
        return Ok(brugere);
    }
    [HttpGet("køkkenchefer")]
    public async Task<ActionResult<List<Bruger>>> HentAlleKøkkenchefer()
    {
        var brugere = await _repo.HentAlle();
        var kokke = brugere.Where(b => b.Rolle == "Køkkenchef").ToList();
        return Ok(kokke);
    }
    [HttpGet("lokationer")]
    public async Task<ActionResult<List<Lokation>>> HentAlleLokationer()
    {
        var lokationer = await _repo.HentAlleLokationer();
        return Ok(lokationer);
    }


}