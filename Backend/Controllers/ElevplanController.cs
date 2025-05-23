using Backend.Repositories.Interface;
using Microsoft.AspNetCore.Mvc;
using Shared;
using Shared.ViewModeller;

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
        [FromQuery] string? filterStatus) // Statusfilter: true = gennemført, false = ikke gennemført
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

        // Brug HOME fra Azure, ellers lokal fallback
        var root = Environment.GetEnvironmentVariable("HOME") ?? Directory.GetCurrentDirectory();
        var uploadFolder = Path.Combine(root, "billeder");

        if (!Directory.Exists(uploadFolder))
            Directory.CreateDirectory(uploadFolder);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadFolder, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return Ok(fileName); // kun filnavn returneres
    }
    [HttpGet("hent-kommentarbillede/{filnavn}")]
    public IActionResult HentKommentarBillede(string filnavn)
    {
        var root = Environment.GetEnvironmentVariable("HOME") ?? Directory.GetCurrentDirectory();
        var uploadFolder = Path.Combine(root, "billeder");
        var filePath = Path.Combine(uploadFolder, filnavn);

        if (!System.IO.File.Exists(filePath))
            return NotFound();

        var contentType = "image/" + Path.GetExtension(filePath).TrimStart('.'); 
        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return File(stream, contentType);
    }
[HttpGet("visningsdeadlines/{brugerId}")]
public async Task<ActionResult<List<DelmaalDeadlineVisning>>> HentDeadlinesSomVisning(int brugerId)
{
    try{
    //opretter en tom liste som bruges til at samle alle visnings-objekter til deadline
    var visninger = new List<DelmaalDeadlineVisning>();
    var idag = DateOnly.FromDateTime(DateTime.Today);

    var alleBrugere = await brugereRepo.HentAlle();
    //finder bruger som er logget ind (brugerId) - dette bruges til at styre hvad der skal indgå i listen
    var currentUser = alleBrugere.FirstOrDefault(b => b._id == brugerId);

    if (currentUser == null)
        return BadRequest("Bruger ikke fundet.");

    foreach (var elev in alleBrugere)
    {
        //Henter hele elevplanen for alle elever (-1 sørger for vi får alle perioder)
        var plan = await elevplanRepo.HentElevplanMedMaal(elev._id, -1);
        if (plan == null) continue;

        bool erKoekkenchefForElev = plan.Ansvarlig?._id == currentUser._id;

        var delmaalListe = plan.ListPerioder
            .SelectMany(p => p.ListMaal)
            .SelectMany(m => m.ListDelmaal);

        IEnumerable<Delmaal> relevante = Enumerable.Empty<Delmaal>();

        //hvis rolle = adin eller hr, så samles alle delmål som er overskredne og ikke gennemført
        if (currentUser.Rolle is "Admin" or "HR")
        {
            relevante = delmaalListe.Where(d => d.Deadline.HasValue && d.Deadline.Value < idag && !d.Status);
        }
        //ellers samles alle delmål som ikke er gennemført - og som senest er 30 dage i fremtiden
        else if (currentUser.Rolle == "Køkkenchef" && erKoekkenchefForElev)
        {
            relevante = delmaalListe.Where(d =>
                d.Deadline.HasValue &&
                !d.Status &&
                (
                    d.Deadline.Value < idag || 
                    (d.Deadline.Value >= idag && d.Deadline.Value <= idag.AddDays(30)) 
                ));
        }
        else if (currentUser.Rolle == "Elev" && elev._id == currentUser._id)
        {
            relevante = delmaalListe.Where(d =>
                d.Deadline.HasValue &&
                !d.Status &&
                d.Deadline.Value >= idag &&
                d.Deadline.Value <= idag.AddDays(7));
        }
        
        string lokationNavn = "Ukendt";
        if (elev.AfdelingId.HasValue)
        {
            var lokation = await lokationRepo.HentLokationViaId(elev.AfdelingId.Value);
            lokationNavn = lokation?.LokationNavn ?? "Ukendt";
        }
    
        relevante = relevante.OrderBy(d => d.Deadline).ToList(); //sikrer at vi sorterer i korrekt rækkefølge

        foreach (var d in relevante)
        {
            visninger.Add(new Shared.ViewModeller.DelmaalDeadlineVisning
            {
                ElevNavn = elev.Navn,
                Lokation = lokationNavn,
                Erhverv = elev.Erhverv ?? "Ukendt",
                DelmaalTitel = d.Titel,
                Deadline = d.Deadline,
                ErOverskredet = d.Deadline < idag,
                DageOverskredet = d.Deadline < idag ? idag.DayNumber - d.Deadline.Value.DayNumber : null,
                AntalDageTilDeadline = d.Deadline >= idag ? d.Deadline.Value.DayNumber - idag.DayNumber : null
            });
        }
    }
    //returnerer listen af visnings-objekter som bruges til frotend
    return Ok(visninger);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[HentDeadlines] FEJL: {ex.Message}");
        return BadRequest("Kunne ikke hente deadlines objekter.");
    }
}
}