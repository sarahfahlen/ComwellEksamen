using Shared;
namespace Backend.Repositories.Interface;

public interface ILokationRepository
{
    Task<List<Shared.Lokation>> HentLokationerAfType(string type);
    
    Task<Lokation?> HentLokationViaId(int id);

}