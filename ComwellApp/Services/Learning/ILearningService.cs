using Shared;
namespace ComwellApp.Services.Learning;

public interface ILearningService
{
    public Task<List<Fagomraade>> HentMockLearning();
}