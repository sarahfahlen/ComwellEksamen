using Shared; 
namespace Backend.Repositories.Interface;

public interface IBrugereRepository
{
    Task TilfoejElev(Bruger nyBruger);

}