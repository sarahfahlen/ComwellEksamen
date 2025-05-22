using Backend.Repositories.Interface;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Shared;

namespace Backend.Controllers;

// Her fortæller vi, at det her er en API-controller – det betyder, at den håndterer HTTP-kald fra frontend
[ApiController]
// Ruten starter med /api/brugere – det er den base-url der skal bruges for at ramme denne controller
[Route("api/learning")]
public class LearningController : ControllerBase
{
    private readonly ILearningRepository _repository;

    public LearningController(ILearningRepository repository)
    {
        _repository = repository;
    }
    
    [HttpGet]
    public async Task<ActionResult<List<Fagomraade>>> HentAlleFagomraader()
    {
        var result = await _repository.HentAlleFagomraader();
        return Ok(result);
    }
}