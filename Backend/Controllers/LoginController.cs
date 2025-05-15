using Backend.Repositories.Interface;
using Microsoft.AspNetCore.Mvc;
using Shared;

namespace Backend.Controllers
{
    // Vi fortæller at dette er en API-controller som kan modtage HTTP-anmodninger fra frontend
    [ApiController]

    // Base-ruten bliver /api/users – alt herinde kaldes med den base
    [Route("api/users")]
    public class LoginController : ControllerBase
    {
        // Vi bruger repository-mønstret til at adskille logik fra databasen – loginRepo styrer det
        private readonly ILoginRepository loginRepo;

        // Repo'en bliver "injiceret" automatisk når controlleren oprettes
        public LoginController(ILoginRepository loginRepo)
        {
            this.loginRepo = loginRepo;
        }

        // POST: /api/users/login
        // Bruges når en bruger logger ind – frontend sender email og adgangskode i body
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] ILoginRepository.LoginRequest loginRequest)
        {
            // Tjek om data mangler – hvis enten email eller adgangskode ikke er sat, sender vi en 401 Unauthorized
            if (loginRequest == null || string.IsNullOrWhiteSpace(loginRequest.Email) || string.IsNullOrWhiteSpace(loginRequest.Adgangskode))
                return Unauthorized("Email og adgangskode skal udfyldes");

            // Vi sender login-oplysningerne til repo'en som validerer dem imod databasen
            var bruger = await loginRepo.Validering(loginRequest.Email, loginRequest.Adgangskode);

            // Hvis login fejler, returnerer vi igen 401 Unauthorized med besked
            if (bruger == null)
                return Unauthorized("Forkert email eller adgangskode");

            // Ellers sender vi brugeren tilbage til frontend med 200 OK – herfra gemmes de f.eks. i LocalStorage
            return Ok(bruger);
        }

        // GET: /api/users
        // Returnerer alle brugere – bruges f.eks. til Admin-sider for at vise overblik
        [HttpGet]
        public IActionResult GetAllUsers()
        {
            // Henter brugere fra repo (det er et almindeligt array)
            var brugere = loginRepo.HentAlleBrugere();

            // Vi fjerner adgangskoden fra hver bruger af sikkerhedshensyn
            foreach (var b in brugere)
                b.Adgangskode = null;

            // Sender alle brugere tilbage med 200 OK
            return Ok(brugere);
        }

        // PUT: /api/users/{id}
        // Bruges til at opdatere en eksisterende bruger med nye oplysninger (f.eks. navn eller rolle)
        [HttpPut("{id}")]
        public async Task<IActionResult> OpdaterBruger(int id, [FromBody] Bruger opdateretBruger)
        {
            // Vi prøver først at finde brugeren i databasen
            var eksisterende = await loginRepo.HentBrugerViaIdAsync(id);
            if (eksisterende == null)
                return NotFound("Brugeren blev ikke fundet");

            // Vi sender den opdaterede bruger videre til repo’en for at blive gemt
            var succes = await loginRepo.OpdaterBrugerAsync(id, opdateretBruger);

            // Hvis noget går galt under opdateringen, returnerer vi en 400 Bad Request
            if (!succes)
                return BadRequest("Kunne ikke opdatere brugeren");

            return Ok(); // Ellers alt godt!
        }

        // PUT: /api/users/{id}/skiftkode
        // Bruges når en bruger selv skifter adgangskode fra frontend
        [HttpPut("{id}/skiftkode")]
        public async Task<IActionResult> SkiftAdgangskode(int id, [FromBody] SkiftKodeRequest request)
        {
            // Vi henter brugeren ud fra ID
            var bruger = await loginRepo.HentBrugerViaIdAsync(id);

            if (bruger == null)
                return NotFound("Bruger findes ikke");

            // Tjek at den nuværende adgangskode matcher det der er gemt i databasen
            if (bruger.Adgangskode != request.NuværendeKode)
                return Unauthorized("Forkert nuværende adgangskode");

            // Hvis koden er korrekt, sætter vi den nye adgangskode
            bruger.Adgangskode = request.NyKode;

            // Gemmer den opdaterede bruger i databasen
            var succes = await loginRepo.OpdaterBrugerAsync(id, bruger);

            if (!succes)
                return BadRequest("Kunne ikke opdatere adgangskoden");

            return Ok(); // Skiftet lykkedes
        }

        // Klasse som matcher det JSON body frontend sender ind, når adgangskode skal skiftes
        public class SkiftKodeRequest
        {
            public string NuværendeKode { get; set; }
            public string NyKode { get; set; }
        }
    }
}
