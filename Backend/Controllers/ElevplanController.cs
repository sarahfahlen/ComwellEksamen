using Backend.Repositories.Interface;
using ComwellApp.Pages;
using Microsoft.AspNetCore.Mvc;
using Shared;

namespace Backend.Controllers;

[ApiController]
[Route("api/elevplan")]
public class ElevplanController : ControllerBase
{
    private readonly IElevplanRepository elevplanRepo;
    private readonly IBrugereRepository brugereRepo;
    private readonly ILokationRepository lokationRepo;

    public ElevplanController(IElevplanRepository elevplanRepo, IBrugereRepository brugereRepo, ILokationRepository lokationRepo)
    {
        this.elevplanRepo = elevplanRepo;
        this.brugereRepo = brugereRepo;
        this.lokationRepo = lokationRepo;
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

    //Opdaterer status på det delmål der er sendt med fra vores service, ved at kalde funktion fra repo
    [HttpPut("statusopdatering/{elevplanId:int}")]
    public async Task<IActionResult> OpdaterStatus(int elevplanId, [FromBody] Delmaal delmaal)
    {
        try
        {
            await elevplanRepo.OpdaterStatusAsync(elevplanId, delmaal);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest($"Fejl: {ex.Message}");
        }
    }

    [HttpPut("kommentar/{elevplanId:int}/{delmaalId:int}/{kommentarId:int}")]
    public async Task<IActionResult> RedigerKommentar(int elevplanId, int delmaalId, int kommentarId,
        [FromBody] Kommentar redigeretKommentar)
    {
        try
        {
            await elevplanRepo.RedigerKommentarAsync(elevplanId, delmaalId, kommentarId, redigeretKommentar);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest($"Fejl ved redigering af kommentar: {ex.Message}");
        }
    }


    // Hente filtrerede mål fra en elevplan via query-parametre
    [HttpGet("filtreredemaal")]
    public async Task<ActionResult<List<Maal>>> HentFiltreredeMaal(
        [FromQuery] int brugerId, // Brugerens ID – bruges til at finde den rigtige elevplan
        [FromQuery] int periodeIndex, // Index for den praktikperiode der ønskes (0, 1, 2, ...)
        [FromQuery] string? valgtMaalNavn, // Valgt mål-navn (bruges som filter)
        [FromQuery] string? valgtDelmaalType, // Valgt type af delmål (fx "Intro", "Kursus" – bruges som filter)
        [FromQuery] string? soegeord, // Søgeord til fritekstsøgning i delmålstitler
        [FromQuery] bool? filterStatus) // Statusfilter: true = gennemført, false = ikke gennemført
    {
        try
        {
            // Kalder repository for at hente filtrerede mål ud fra parametrene
            var resultater = await elevplanRepo.HentFiltreredeMaal(
                brugerId, periodeIndex, valgtMaalNavn, valgtDelmaalType, soegeord, filterStatus);


            // Returnerer listen med mål (OK = 200)
            return Ok(resultater);
        }
        catch (Exception ex)
        {
            // Hvis noget går galt, returneres en fejlbesked (400 Bad Request)
            Console.WriteLine($"[HentFiltreredeMaal] FEJL: {ex.Message}");
            return BadRequest(ex.Message);
        }
    }


    [HttpPost("delmaal/{elevplanId:int}/{maalId:int}")]
    public async Task<IActionResult> TilfoejDelmaal(int elevplanId, int maalId, [FromBody] Delmaal nytDelmaal)
    {
        try
        {
            await elevplanRepo.TilfoejDelmaal(elevplanId, maalId, nytDelmaal);
            return Ok("Delmål tilføjet.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TilfoejDelmaal] FEJL: {ex.Message}");
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("delmaal/{elevplanId:int}/{periodeIndex:int}/{maalId:int}/{delmaalId:int}")]
    public async Task<IActionResult> OpdaterDelmaal(int elevplanId, int periodeIndex, int maalId, int delmaalId,
        [FromBody] Delmaal opdateretDelmaal)
    {
        try
        {
            await elevplanRepo.OpdaterDelmaal(elevplanId, periodeIndex, maalId, delmaalId, opdateretDelmaal);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest($"Fejl ved opdatering af delmål: {ex.Message}");
        }
    }
    
    [HttpDelete("delmaal/{elevplanId:int}/{periodeIndex:int}/{maalId:int}/{delmaalId:int}")]
    public async Task<IActionResult> SletDelmaal(int elevplanId, int periodeIndex, int maalId, int delmaalId)
    {
        try
        {
            await elevplanRepo.SletDelmaal(elevplanId, periodeIndex, maalId, delmaalId);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest($"Fejl ved sletning af delmål: {ex.Message}");

        }
    }



    [HttpGet("maal/{elevplanId:int}/{periodeIndex:int}")]
    public async Task<ActionResult<List<Maal>>> HentMaal(int elevplanId, int periodeIndex)
    {
        var elevplan = await elevplanRepo.HentElevplanMedMaal(elevplanId, periodeIndex);
        if (elevplan == null)
            return NotFound("Elevplan ikke fundet");

        var periode = elevplan.ListPerioder.ElementAtOrDefault(periodeIndex);
        return periode?.ListMaal ?? new List<Maal>();
    }

    [HttpGet("delmaaltyper/{elevplanId:int}/{periodeIndex:int}")]
    public async Task<ActionResult<List<string>>> HentUnikkeDelmaalTyper(int elevplanId, int periodeIndex)
    {
        var elevplan = await elevplanRepo.HentElevplanMedMaal(elevplanId, periodeIndex);
        if (elevplan == null)
            return NotFound();

        var typer = elevplan.ListPerioder.ElementAtOrDefault(periodeIndex)?
            .ListMaal
            .SelectMany(m => m.ListDelmaal)
            .Select(d => d.DelmaalType)
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Distinct()
            .ToList();

        return typer ?? new List<string>();
    }

    [HttpGet("kommendedeadlines/{brugerId:int}")]
    public async Task<ActionResult<List<Delmaal>>> HentKommendeDeadlines(int brugerId)
    {
        try
        {
            var iDag = DateOnly.FromDateTime(DateTime.Today);
            var om7Dage = iDag.AddDays(7);

            var elevplan = await elevplanRepo.HentElevplanMedMaal(brugerId, -1); // -1 = alle perioder
            if (elevplan == null)
                return NotFound("Elevplan ikke fundet.");

            var delmaal = elevplan.ListPerioder
                .SelectMany(p => p.ListMaal)
                .SelectMany(m => m.ListDelmaal)
                .Where(d => d.Deadline.HasValue &&
                            !d.Status &&
                            d.Deadline.Value >= iDag &&
                            d.Deadline.Value <= om7Dage)
                .OrderBy(d => d.Deadline)
                .ToList();

            return Ok(delmaal);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[HentKommendeDeadlines] FEJL: {ex.Message}");
            return BadRequest("Kunne ikke hente kommende deadlines.");
        }
    }
    [HttpPut("igangopdatering/{elevplanId:int}")]
    public async Task<IActionResult> OpdaterIgang(int elevplanId, [FromBody] Delmaal delmaal)
    {
        try
        {
            await elevplanRepo.OpdaterIgangAsync(elevplanId, delmaal);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest($"Fejl: {ex.Message}");
        }
    }
    [HttpPost("upload-kommentarbillede")]
    public async Task<IActionResult> UploadKommentarBillede()
    {
        var file = Request.Form.Files.FirstOrDefault();
        if (file == null || file.Length == 0)
            return BadRequest("Ingen fil modtaget.");

        // Opret mappe hvis den ikke findes
        var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "billeder");
        if (!Directory.Exists(uploadsPath))
            Directory.CreateDirectory(uploadsPath);

        // Generer unik filnavn
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadsPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Returnér den relative sti som gemmes i databasen
        
        var relativePath = Path.Combine("uploads", "billeder", fileName).Replace("\\", "/");
        return Ok(relativePath);
    }
   
    [HttpGet("deadlines-raw/{adminId}")]
    public async Task<ActionResult<Dictionary<string, List<Delmaal>>>> HentDeadlinesRaw(int adminId)
    {
        try
        {
            var idag = DateOnly.FromDateTime(DateTime.Today);
            var resultat = new Dictionary<string, List<Delmaal>>();

            var brugere = await brugereRepo.HentAlle();

            foreach (var bruger in brugere)
            {
                var plan = await elevplanRepo.HentElevplanMedMaal(bruger._id, 0); // midlertidig
                if (plan == null || plan.ListPerioder == null) continue;

                foreach (var periode in plan.ListPerioder)
                {
                    var delmaalListe = periode.ListMaal
                        .SelectMany(m => m.ListDelmaal)
                        .Where(d => d.Deadline.HasValue && !d.Status)
                        .ToList();

                    if (delmaalListe.Any())
                    {
                        if (!resultat.ContainsKey(bruger.Navn))
                            resultat[bruger.Navn] = new List<Delmaal>();

                        resultat[bruger.Navn].AddRange(delmaalListe);
                    }
                }
            }

            return Ok(resultat);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[HentDeadlinesRaw] FEJL: {ex.Message}");
            return BadRequest("Kunne ikke hente deadlines.");
        }
    }
    [HttpGet("visningsdeadlines/{brugerId}")]
    public async Task<ActionResult<List<DeadlinesPage.DelmaalVisning>>> HentDeadlinesSomVisning(int brugerId)
    {
        var visninger = new List<DeadlinesPage.DelmaalVisning>();
        var idag = DateOnly.FromDateTime(DateTime.Today);

        var alleBrugere = await brugereRepo.HentAlle();
        var currentUser = alleBrugere.FirstOrDefault(b => b._id == brugerId);

        if (currentUser == null)
            return BadRequest("Bruger ikke fundet.");

        foreach (var elev in alleBrugere)
        {
            var plan = await elevplanRepo.HentElevplanMedMaal(elev._id, -1);
            if (plan == null) continue;

            bool erKoekkenchefForElev = plan.Ansvarlig?._id == currentUser._id;

            var delmaalListe = plan.ListPerioder
                .SelectMany(p => p.ListMaal)
                .SelectMany(m => m.ListDelmaal);

            IEnumerable<Delmaal> relevante = Enumerable.Empty<Delmaal>();

            if (currentUser.Rolle is "Admin" or "HR")
            {
                relevante = delmaalListe.Where(d => d.Deadline.HasValue && d.Deadline.Value < idag && !d.Status);
            }
            else if (currentUser.Rolle == "Køkkenchef" && erKoekkenchefForElev)
            {
                relevante = delmaalListe.Where(d => d.Deadline.HasValue && !d.Status);
            }

            string lokationNavn = "Ukendt";
            if (elev.AfdelingId.HasValue)
            {
                var lokation = await lokationRepo.HentLokationViaId(elev.AfdelingId.Value);
                lokationNavn = lokation?.LokationNavn ?? "Ukendt";
            }

            foreach (var d in relevante)
            {
                visninger.Add(new DeadlinesPage.DelmaalVisning
                {
                    Navn = elev.Navn,
                    Lokation = lokationNavn,
                    Erhverv = elev.Erhverv ?? "Ukendt",
                    Titel = d.Titel,
                    Deadline = d.Deadline,
                    DeadlineKommentar = d.DeadlineKommentar,
                    ErOverskredet = d.Deadline < idag
                });
            }
        }

        return Ok(visninger);
    }



}