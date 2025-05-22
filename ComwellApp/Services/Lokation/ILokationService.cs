using Shared;

namespace ComwellApp.Services.Lokation;

public interface ILokationService
{
    Task<List<Shared.Lokation>> HentKoekkenLokationer();
    Task<List<Shared.Lokation>> HentSkoleLokationer();
    
    Task<Shared.Lokation?> HentLokationViaId(int id);
}