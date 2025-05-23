using Backend.Repositories.Interface;
using backend.Services;
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
    
    [HttpPut("arkiver")]
    public async Task<IActionResult> ArkiverElev([FromBody] Bruger elev)
    {
        try
        {
            await _repo.ArkiverElev(elev);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest($"Kunne ikke arkivere elev: {ex.Message}");
        }
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
                .Where(b => b.AfdelingId == lokationId)
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
    
    // Henter brugerens elevplan ud fra brugerId
    [HttpGet("{brugerId}/elevplan")]
    public async Task<ActionResult<Elevplan>> HentElevplanForBruger(int brugerId, [FromQuery] int forespoergerId)
    {
        var plan = await _repo.HentElevplanForBruger(brugerId, forespoergerId);
        if (plan == null)
            return Forbid("Du har ikke adgang til denne elevplan");

        return Ok(plan);
    }

    
    [HttpGet("filtreredeelever")]
    public async Task<ActionResult<List<Bruger>>> HentFiltreredeElever(
        [FromQuery] string? soegeord,
        [FromQuery] string? kursus,
        [FromQuery] string? erhverv,
        [FromQuery] int? deadline,
        [FromQuery] string? rolle,
        [FromQuery] string? status,
        [FromQuery] string? afdelingId,
        [FromQuery] string? aktiv)
    {
        // Pars afdelingId fra string → int?
        int? parsedAfdelingId = null;
        if (int.TryParse(afdelingId, out int temp))
        {
            parsedAfdelingId = temp;
        }
        
        bool? parsedAktiv = null;
        if (bool.TryParse(aktiv, out bool aktivBool))
        {
            parsedAktiv = aktivBool;
        }

        // Kald repository med parsedAfdelingId
        var elever = await _repo.HentFiltreredeElever(
            soegeord ?? "",
            kursus ?? "",
            erhverv ?? "",
            deadline,
            rolle ?? "",
            status ?? "",
            parsedAfdelingId,
            parsedAktiv);

        return Ok(elever);
    }
    
    //Bliver brugt til at opdater skole lokationen
    [HttpPut("{id}")]
    public async Task<IActionResult> OpdaterBruger(int id, [FromBody] Bruger bruger)
    {
        await _repo.OpdaterBruger(bruger);
        return Ok();
    }

    // Endpoint til at opdatere SkoleId i en specifik praktikperiode
    [HttpPut("{brugerId}/skole")]
    public async Task<IActionResult> OpdaterSkoleId(
        int brugerId,                // ID på brugeren der skal opdateres
        [FromQuery] int periodeIndex, // Hvilken praktikperiode det gælder (0, 1, 2, ...)
        [FromQuery] int? nySkoleId)   // Den nye skoleId vi ønsker at sætte (kan være null for at rydde feltet)
    {
        try
        {
            // Kalder repository-metoden, som opdaterer feltet i MongoDB
            await _repo.OpdaterSkoleId(brugerId, periodeIndex, nySkoleId);

            // Returnerer 200 OK hvis det lykkes
            return Ok();
        }
        catch (Exception ex)
        {
            // Hvis noget fejler – fx bruger ikke fundet – send 400 BadRequest med fejlbesked
            return BadRequest($"Fejl ved opdatering af SkoleId: {ex.Message}");
        }
    }
    
    [HttpGet("eksporter-elever")]
    public async Task<IActionResult> EksporterEleverTilExcel(
        [FromQuery] string? soegeord,
        [FromQuery] string? kursus,
        [FromQuery] string? erhverv,
        [FromQuery] int? deadline,
        [FromQuery] string? rolle,
        [FromQuery] string? status,
        [FromQuery] int? afdelingId,
        [FromQuery] string? aktiv,
        [FromServices] ExcelEksportService excelService)
    {
        bool? parsedAktiv = null;
        if (bool.TryParse(aktiv, out var aktivBool))
            parsedAktiv = aktivBool;

        var elever = await _repo.HentFiltreredeElever(
            soegeord ?? "",
            kursus ?? "",
            erhverv ?? "",
            deadline,
            rolle ?? "Elev",
            status ?? "",
            afdelingId,
            parsedAktiv);

        var excelBytes = excelService.GenererExcelMedNavne(elever);

        return File(
            excelBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "Elever.xlsx");
    }


    [HttpGet("erhverv")]
    public async Task<ActionResult<List<string>>> HentErhverv()
    {
        try
        {
            var erhverv = await _repo.HentErhverv();
            return Ok(erhverv);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Fejl ved hentning af erhverv: {ex.Message}");
        }
    }
    
    [HttpGet("kurser")]
    public async Task<ActionResult<List<string>>> HentKurser()
    {
        try
        {
            var kurser = await _repo.HentKurser();
            return Ok(kurser);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Fejl ved hentning af kurser: {ex.Message}");
        }
    }
    [HttpPost("upload-profilbillede")]
    public async Task<IActionResult> UploadProfilBillede([FromQuery] int brugerId)
    {
        var file = Request.Form.Files.FirstOrDefault();
        if (file == null || file.Length == 0)
            return BadRequest("Intet billede valgt");

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(fileExtension))
            return BadRequest("Kun JPG og PNG filer er tilladt.");

        // GEMMER IKKE I wwwroot, men i Azure HOME (fx /home/billeder/brugere)
        var root = Environment.GetEnvironmentVariable("HOME") ?? Directory.GetCurrentDirectory();
        var uploadsFolder = Path.Combine(root, "billeder", "brugere");

        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        // Gem kun filnavn (ikke hele path)
        var relativePath = $"billeder/brugere/{uniqueFileName}";

        await _repo.OpdaterBillede(brugerId, relativePath);

        return Ok(relativePath);
    }
    [HttpGet("hent-profilbillede/{filnavn}")]
    public IActionResult HentProfilBillede(string filnavn)
    {
        var root = Environment.GetEnvironmentVariable("HOME") ?? Directory.GetCurrentDirectory();
        var uploadFolder = Path.Combine(root, "billeder", "brugere");
        var filePath = Path.Combine(uploadFolder, filnavn);

        if (!System.IO.File.Exists(filePath))
            return NotFound();

        var contentType = "image/" + Path.GetExtension(filePath).TrimStart('.');
        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return File(stream, contentType);
    }

    [HttpDelete("slet-billede")]
    public IActionResult SletProfilbillede([FromQuery] string sti)
    {
        if (string.IsNullOrWhiteSpace(sti))
            return BadRequest("Ingen sti angivet.");

        var filnavn = Path.GetFileName(sti); // sikrer os mod stier udenfor mappen

        var root = Environment.GetEnvironmentVariable("HOME") ?? Directory.GetCurrentDirectory();
        var uploadFolder = Path.Combine(root, "billeder", "brugere");
        var filePath = Path.Combine(uploadFolder, filnavn);

        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
            return Ok("Billede slettet.");
        }

        return NotFound("Filen blev ikke fundet.");
    }

}
