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
    public List<Shared.Elevplan> GetAllElevplaner()
    {
        return alleElevplaner;
    }


    //Opretter en ny elevplan, ved at kalde vores skabelon funktion og sende bruger + ansvarlig med
    public async Task<Shared.Elevplan> OpretElevplan(Bruger ansvarlig)
    {
        var plan = await LavDefaultSkabelon(ansvarlig);
        plan.ElevplanId = _idGenerator.GenererNytId(alleElevplaner, p => p.ElevplanId);
        alleElevplaner.Add(plan);
        return plan;
    }

    //Returnerer vores skabelon, og tilknytter den medsendte elev og ansvarlige fra OpretElevplan() til elev og ansvarlig felter
    public async Task<Shared.Elevplan> LavDefaultSkabelon(Bruger ansvarlig)
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
                                    DelmaalID = 1,
                                    DelmaalType = "Intro",
                                    Beskrivelse = "Udlever tøj og sikkerhedssko",
                                    Ansvarlig = "Elevansvarlig/Nærmeste leder",
                                    Deadline = new DateOnly(2025, 8, 8),
                                    DeadlineKommentar = "",
                                    Status = false,
                                },
                                new Delmaal
                                {
                                    DelmaalID = 2,
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
                                    DelmaalID = 3,
                                    DelmaalType = "Intro",
                                    Beskrivelse = "Vagtplaner",
                                    Ansvarlig = "Elevansvarlig/Nærmeste leder",
                                    Deadline = new DateOnly(2025, 8, 15),
                                    DeadlineKommentar = "",
                                    Status = false,
                                },
                                new Delmaal
                                {
                                    DelmaalID = 4,
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
                                    DelmaalID = 5,
                                    DelmaalType = "Intro",
                                    Beskrivelse = "Intro til arbejdsmiljø på Comwell Connect",
                                    Ansvarlig = "Elevansvarlig/Nærmeste leder",
                                    Deadline = new DateOnly(2025, 8, 31),
                                    DeadlineKommentar = "",
                                    Status = false,
                                },
                                new Delmaal
                                {
                                    DelmaalID = 6,
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
                                    DelmaalID = 7,
                                    DelmaalType = "Samtale",
                                    Beskrivelse = "6 ugers samtale",
                                    Ansvarlig = "Elevansvarlig/Nærmeste leder",
                                    Deadline = new DateOnly(2025, 9, 12),
                                    DeadlineKommentar = "",
                                    Status = false,
                                },
                                new Delmaal
                                {
                                    DelmaalID = 8,
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
                                    DelmaalID = 9,
                                    DelmaalType = "Kursus",
                                    Beskrivelse = "Kernen i Comwell - intro",
                                    Ansvarlig = "Elevansvarlig/Nærmeste leder",
                                    Deadline = null,
                                    DeadlineKommentar = "efter prøvetid",
                                    Status = false,
                                },
                                new Delmaal
                                {
                                    DelmaalID = 10,
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
                                    DelmaalID = 11,
                                    DelmaalType = "Fagligt mål",
                                    Beskrivelse = "Kendskab til systemer",
                                    Ansvarlig = "Elevansvarlig/Nærmeste leder",
                                    Deadline = new DateOnly(2025, 9, 22),
                                    DeadlineKommentar = "",
                                    Status = false,
                                },
                                new Delmaal
                                {
                                    DelmaalID = 12,
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
                                    DelmaalID = 13,
                                    DelmaalType = "Intro",
                                    Beskrivelse = "Kendskab til Esmiley",
                                    Ansvarlig = "Elevansvarlig/Nærmeste leder",
                                    Deadline = new DateOnly(2025, 10, 30),
                                    DeadlineKommentar = "",
                                    Status = false,
                                },
                                new Delmaal
                                {
                                    DelmaalID = 14,
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
}