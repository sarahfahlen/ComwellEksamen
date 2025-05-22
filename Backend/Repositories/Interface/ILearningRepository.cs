using Shared;

namespace Backend.Repositories.Interface;

public interface ILearningRepository
{
    Task<List<Fagomraade>> HentAlleFagomraader();
 
}