using Shared;

namespace ComwellApp.Services.Elevplan;

public class ElevplanServiceMock : IElevplanService
{
    private readonly IdGeneratorService _idGenerator;

    public ElevplanServiceMock(IdGeneratorService idGenerator)
    {
        _idGenerator = idGenerator;
    }

    private List<Shared.Elevplan> alleElevplaner = new();
    
    public Task TilfoejKommentar(Shared.Elevplan minPlan, int delmaalId, Kommentar nyKommentar)
    {
        //Her finder vi alle delmål i den elevplan der sendes med og derefter det specifikke delmål ud fra ID
        var delmaal = minPlan.ListPerioder
            .SelectMany(p => p.ListMaal)
            .SelectMany(m => m.ListDelmaal)
            .FirstOrDefault(d => d.DelmaalId == delmaalId);
        
        if (delmaal != null)
        {
            //Genererer et ID til den nye kommentar via vores service
            nyKommentar.KommentarId = _idGenerator.GenererNytId(delmaal.Kommentarer, k => k.KommentarId);
            nyKommentar.Dato = DateOnly.FromDateTime(DateTime.Today);
            
            //kommentar tilføjes
            delmaal.Kommentarer.Add(nyKommentar);
        }
        return Task.CompletedTask;
    }
    
    public Task RedigerKommentar(Shared.Elevplan minPlan, int delmaalId, int kommentarId, string nyTekst)
    {
        //Her finder vi alle delmål i den elevplan der sendes med og derefter det specifikke delmål ud fra ID
        var delmaal = minPlan.ListPerioder
            .SelectMany(p => p.ListMaal)
            .SelectMany(m => m.ListDelmaal)
            .FirstOrDefault(d => d.DelmaalId == delmaalId);

        //Her finder vi den kommentar vi vil redigere/opdatere, ud fra ID 
        var kommentar = delmaal?.Kommentarer.FirstOrDefault(k => k.KommentarId == kommentarId);

        //Her laver vi opdateringen, baseret på den nye tekst, og sætter datoen til i dag
        if (kommentar != null)
        {
            kommentar.Tekst = nyTekst;
            kommentar.Dato = DateOnly.FromDateTime(DateTime.Today); 
        }
        return Task.CompletedTask;
    }
    
    public Task<Kommentar?> GetKommentarAsync(int elevplanId, int delmaalId, string brugerRolle)
    {
        throw new NotImplementedException();
    }

    public Task OpdaterStatus(Shared.Elevplan plan, Delmaal delmaal)
    {
        throw new NotImplementedException();
    }

    public List<Maal> HentFiltreredeMaal(Shared.Elevplan plan, int periodeIndex, string? valgtMaalNavn, string? valgtDelmaalType, string? søgeord, bool? filterStatus)
    {
        if (plan == null || plan.ListPerioder.Count <= periodeIndex)
            return new List<Maal>();

        var periode = plan.ListPerioder[periodeIndex];
        var søg = søgeord?.ToLower() ?? "";

        return periode.ListMaal
            .Where(m => string.IsNullOrWhiteSpace(valgtMaalNavn) || m.MaalNavn == valgtMaalNavn)
            .Select(m => new Maal
            {
                MaalId = m.MaalId,
                MaalNavn = m.MaalNavn,
                ListDelmaal = m.ListDelmaal
                    .Where(d =>
                        (string.IsNullOrWhiteSpace(valgtDelmaalType) || d.DelmaalType == valgtDelmaalType) &&
                        (string.IsNullOrWhiteSpace(søg) || d.Titel.ToLower().Contains(søg)) &&
                        (filterStatus == null || d.Status == filterStatus)
                    )
                    .ToList()
            })
            .Where(m => m.ListDelmaal.Any())
            .ToList();
    }


    //Opretter en ny elevplan, ved at kalde vores skabelon funktion og sende bruger + ansvarlig med
    public async Task<Shared.Elevplan> OpretElevplan(Bruger ansvarlig, string skabelonNavn)
    {
        var plan = await LavDefaultSkabelon(ansvarlig, skabelonNavn);
        plan.ElevplanId = _idGenerator.GenererNytId(alleElevplaner, p => p.ElevplanId);
        alleElevplaner.Add(plan);
        return plan;
    }

    //Returnerer vores skabelon, og tilknytter den medsendte elev og ansvarlige fra OpretElevplan() til elev og ansvarlig felter
    public async Task<Shared.Elevplan> LavDefaultSkabelon(Bruger ansvarlig, string skabelonNavn)
    {
        return new Shared.Elevplan
        {
            ElevplanId = 1,
            Ansvarlig = ansvarlig,
            ListPerioder = new List<Praktikperiode>
            {
                new Praktikperiode
                {
                    PraktikId = 1,
                    PraktikNavn = "Praktikperiode 1",
                    Skolevarighed = 10,
                    Praktikvarighed = 52,
                    StartDato = new DateOnly(2025, 8, 1),
                    SlutDato = new DateOnly(2026, 7, 31),
                    Skole = new Lokation
                    {
                        LokationId = 1,
                        LokationNavn = "Comwell Skole",
                        Adresse = "Skolevej 1",
                        LokationTelefon = "12345678",
                        LokationType = "Skole",
                    },
                    ListMaal = new List<Maal>
                    {
                        new Maal
                        {
                            MaalId = 1,
                            MaalNavn = "Velkommen til og intro",
                            ListDelmaal = new List<Delmaal>
                            {
                                new Delmaal
                                {
                                    DelmaalId = 1,
                                    DelmaalType = "Intro",
                                    Beskrivelse = "Udlever tøj og sikkerhedssko",
                                    Ansvarlig = "Elevansvarlig/Nærmeste leder",
                                    Deadline = new DateOnly(2025, 8, 8),
                                    DeadlineKommentar = "",
                                    Status = false,
                                },
                                new Delmaal
                                {
                                    DelmaalId = 2,
                                    DelmaalType = "Intro",
                                    Beskrivelse = "Fremvisning af områder",
                                    Ansvarlig = "Elevansvarlig/Nærmeste leder",
                                    Deadline = new DateOnly(2025, 8, 8),
                                    DeadlineKommentar = "",
                                    Status = false,
                                },
                            },
                        },
                        new Maal
                        {
                            MaalId = 2,
                            MaalNavn = "Information fra nærmeste leder",
                            ListDelmaal = new List<Delmaal>
                            {
                                new Delmaal
                                {
                                    DelmaalId = 3,
                                    DelmaalType = "Intro",
                                    Beskrivelse = "Vagtplaner",
                                    Ansvarlig = "Elevansvarlig/Nærmeste leder",
                                    Deadline = new DateOnly(2025, 8, 15),
                                    DeadlineKommentar = "",
                                    Status = false,
                                },
                                new Delmaal
                                {
                                    DelmaalId = 4,
                                    DelmaalType = "Intro",
                                    Beskrivelse = "Ferie og fridage",
                                    Ansvarlig = "Elevansvarlig/Nærmeste leder",
                                    Deadline = new DateOnly(2025, 8, 15),
                                    DeadlineKommentar = "",
                                    Status = false,
                                },
                            },
                        },
                        new Maal
                        {
                            MaalId = 3,
                            MaalNavn = "Sikkerhed og arbejdsmiljø",
                            ListDelmaal = new List<Delmaal>
                            {
                                new Delmaal
                                {
                                    DelmaalId = 5,
                                    DelmaalType = "Intro",
                                    Beskrivelse = "Intro til arbejdsmiljø på Comwell Connect",
                                    Ansvarlig = "Elevansvarlig/Nærmeste leder",
                                    Deadline = new DateOnly(2025, 8, 31),
                                    DeadlineKommentar = "",
                                    Status = false,
                                },
                                new Delmaal
                                {
                                    DelmaalId = 6,
                                    DelmaalType = "Intro",
                                    Beskrivelse = "Ergonomi og tunge løft",
                                    Ansvarlig = "Elevansvarlig/Nærmeste leder",
                                    Deadline = new DateOnly(2025, 8, 31),
                                    DeadlineKommentar = "",
                                    Status = false,
                                },
                            },
                        },
                        new Maal
                        {
                            MaalId = 4,
                            MaalNavn = "Samtaler undervejs i periode",
                            ListDelmaal = new List<Delmaal>
                            {
                                new Delmaal
                                {
                                    DelmaalId = 7,
                                    DelmaalType = "Samtale",
                                    Beskrivelse = "6 ugers samtale",
                                    Ansvarlig = "Elevansvarlig/Nærmeste leder",
                                    Deadline = new DateOnly(2025, 9, 12),
                                    DeadlineKommentar = "",
                                    Status = false,
                                },
                                new Delmaal
                                {
                                    DelmaalId = 8,
                                    DelmaalType = "Samtale",
                                    Beskrivelse = "3 måneders samtale",
                                    Ansvarlig = "Elevansvarlig/Nærmeste leder",
                                    Deadline = new DateOnly(2025, 10, 10),
                                    DeadlineKommentar = "",
                                    Status = false,
                                },
                            },
                        },
                        new Maal
                        {
                            MaalId = 5,
                            MaalNavn = "Interne kurser mv.",
                            ListDelmaal = new List<Delmaal>
                            {
                                new Delmaal
                                {
                                    DelmaalId = 9,
                                    DelmaalType = "Kursus",
                                    Beskrivelse = "Kernen i Comwell - intro",
                                    Ansvarlig = "Elevansvarlig/Nærmeste leder",
                                    Deadline = null,
                                    DeadlineKommentar = "efter prøvetid",
                                    Status = false,
                                },
                                new Delmaal
                                {
                                    DelmaalId = 10,
                                    DelmaalType = "Kursus",
                                    Beskrivelse = "Kernen i Comwell - ESG",
                                    Ansvarlig = "Elevansvarlig/Nærmeste leder",
                                    Deadline = null,
                                    DeadlineKommentar = "efter prøvetid",
                                    Status = false,
                                },
                            },
                        },
                        new Maal
                        {
                            MaalId = 6,
                            MaalNavn = "Faglige mål",
                            ListDelmaal = new List<Delmaal>
                            {
                                new Delmaal
                                {
                                    DelmaalId = 11,
                                    DelmaalType = "Fagligt mål",
                                    Beskrivelse = "Kendskab til systemer",
                                    Ansvarlig = "Elevansvarlig/Nærmeste leder",
                                    Deadline = new DateOnly(2025, 9, 22),
                                    DeadlineKommentar = "",
                                    Status = false,
                                },
                                new Delmaal
                                {
                                    DelmaalId = 12,
                                    DelmaalType = "Fagligt mål",
                                    Beskrivelse = "Knives funktionalitet",
                                    Ansvarlig = "Elevansvarlig/Nærmeste leder",
                                    Deadline = null,
                                    DeadlineKommentar = "efter prøvetid",
                                    Status = false,
                                },
                            },
                        },
                        new Maal
                        {
                            MaalId = 7,
                            MaalNavn = "Madspild og affaldssortering",
                            ListDelmaal = new List<Delmaal>
                            {
                                new Delmaal
                                {
                                    DelmaalId = 13,
                                    DelmaalType = "Intro",
                                    Beskrivelse = "Kendskab til Esmiley",
                                    Ansvarlig = "Elevansvarlig/Nærmeste leder",
                                    Deadline = new DateOnly(2025, 10, 30),
                                    DeadlineKommentar = "",
                                    Status = false,
                                },
                                new Delmaal
                                {
                                    DelmaalId = 14,
                                    DelmaalType = "Fagligt mål",
                                    Beskrivelse = "Affaldssortering i køkkenet",
                                    Ansvarlig = "Elevansvarlig/Nærmeste leder",
                                    Deadline = new DateOnly(2025, 10, 30),
                                    DeadlineKommentar = "",
                                    Status = false,
                                },
                            },
                        },
                    },
                }
            }
        };
    }

    public Task<List<Maal>> HentFiltreredeMaal(int brugerId, int periodeIndex, string? valgtMaalNavn, string? valgtDelmaalType,
        string? soegeord, bool? filterStatus)
    {
        throw new NotImplementedException();
    }

    public Task TilfoejDelmaal(Shared.Elevplan plan, int maalId, Delmaal nytDelmaal)
    {
        throw new NotImplementedException();
    }

    public Task OpdaterDelmaal(Shared.Elevplan plan, int periodeIndex, int maalId, Delmaal opdateretDelmaal)
    {
        throw new NotImplementedException();
    }

    public Task<List<Maal>> HentMaalFraPeriode(int elevplanId, int periodeIndex)
    {
        throw new NotImplementedException();
    }

    public Task<List<string>> HentDelmaalTyperFraPeriode(int elevplanId, int periodeIndex)
    {
        throw new NotImplementedException();
    }
}