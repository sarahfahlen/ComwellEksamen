using Backend.Repositories.Interface;
using Microsoft.AspNetCore.Mvc;
using Shared;

namespace Backend.Controllers;

[ApiController]
[Route("api/lokationer")]
public class LokationController : ControllerBase
{
    private readonly ILokationRepository _repo;

    public LokationController(ILokationRepository repo)
    {
        _repo = repo;
    }

    [HttpGet("koekken")]
    public async Task<ActionResult<List<Lokation>>> HentKoekkenLokationer()
    {
        var lokationer = await _repo.HentLokationerAfType("Køkken");
        return lokationer.Any() ? Ok(lokationer) : NotFound("Ingen køkken-lokationer fundet");
    }

    [HttpGet("skole")]
    public async Task<ActionResult<List<Lokation>>> HentSkoleLokationer()
    {
        var lokationer = await _repo.HentLokationerAfType("Skole");
        return lokationer.Any() ? Ok(lokationer) : NotFound("Ingen skole-lokationer fundet");
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<Lokation>> GetLokationById(int id)
    {
        var lokation = await _repo.HentLokationViaId(id);
        if (lokation == null)
            return NotFound();
        return Ok(lokation);
    }

}
