using Backend.Repositories.Interface;
using Microsoft.AspNetCore.Mvc;
using Shared;
namespace Backend.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class LoginController : ControllerBase
    {
        private readonly ILoginRepository loginRepo;

        public LoginController(ILoginRepository loginRepo)
        {
            this.loginRepo = loginRepo;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] ILoginRepository.LoginRequest loginRequest)
        {
            if (loginRequest == null || string.IsNullOrWhiteSpace(loginRequest.Email) || string.IsNullOrWhiteSpace(loginRequest.Adgangskode))
                return Unauthorized("Email og adgangskode skal udfyldes");

            var bruger = await loginRepo.Validering(loginRequest.Email, loginRequest.Adgangskode);

            if (bruger == null)
                return Unauthorized("Forkert email eller adgangskode");

            return Ok(bruger);
            
            
        }
        
        [HttpGet]
        public IActionResult GetAllUsers()
        {
            var brugere = loginRepo.HentAlleBrugere();
            foreach (var b in brugere)
                b.Adgangskode = null;
            return Ok(brugere);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> OpdaterBruger(int id, [FromBody] Bruger opdateretBruger)
        {
            var eksisterende = await loginRepo.HentBrugerViaIdAsync(id);
            if (eksisterende == null)
                return NotFound("Brugeren blev ikke fundet");


            var succes = await loginRepo.OpdaterBrugerAsync(id, opdateretBruger);
            if (!succes)
                return BadRequest("Kunne ikke opdatere brugeren");

            return Ok();
        }
        [HttpPut("{id}/skiftkode")]
        public async Task<IActionResult> SkiftAdgangskode(int id, [FromBody] SkiftKodeRequest request)
        {
            var bruger = await loginRepo.HentBrugerViaIdAsync(id);

            if (bruger == null)
                return NotFound("Bruger findes ikke");

            if (bruger.Adgangskode != request.NuværendeKode)
                return Unauthorized("Forkert nuværende adgangskode");

            bruger.Adgangskode = request.NyKode;

            var succes = await loginRepo.OpdaterBrugerAsync(id, bruger);
            if (!succes)
                return BadRequest("Kunne ikke opdatere adgangskoden");

            return Ok();
        }

        public class SkiftKodeRequest
        {
            public string NuværendeKode { get; set; }
            public string NyKode { get; set; }
        }

    }
}