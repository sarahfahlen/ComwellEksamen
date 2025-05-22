using Shared;
namespace ComwellApp.Services.Learning;

public interface ILearningService
{
    Task<List<Fagomraade>> HentAlleFagomraader();
}