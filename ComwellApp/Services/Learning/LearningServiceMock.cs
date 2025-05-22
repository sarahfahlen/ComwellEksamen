using Shared; 
namespace ComwellApp.Services.Learning;

public class LearningServiceMock : ILearningService
{
    public async Task<List<Fagomraade>> HentAlleFagomraader()
    {
        List<Fagomraade> fagomraader = new List<Fagomraade>();
        var quiz = new Element()
        {
            ElementNavn = "Steak-quiz",
            ElementType = "quiz",
            ListSpoergsmaal = new List<Spoergsmaal>
            {
                new Spoergsmaal
                {
                    Tekst = "Hvilken temperatur er mest passende for medium steak?",
                    KorrektIndex = 1,
                    Svar = new List<Svarmulighed>
                    {
                        new Svarmulighed { Tekst = "48°C" },
                        new Svarmulighed { Tekst = "56°C" },
                        new Svarmulighed { Tekst = "68°C" }
                    }
                }
            }
        };

        var fag = new Fagomraade
        {
            FagomraadeNavn = "Udskæring af okse",
            FagomraadeBeskrivelse = "Lær hvordan man udskærer og tilbereder forskellige udskæringer.",
            ListUnderemne = new List<Underemne>
            {
                new Underemne
                {
                    UnderemneNavn = "Steak",
                    UnderemneBeskrivelse = "Fokus på tilberedning af steaks.",
                    ListElement = new List<Element> { quiz }
                }
            }
        };
        fagomraader.Add(fag);
        return fagomraader;
    }
}