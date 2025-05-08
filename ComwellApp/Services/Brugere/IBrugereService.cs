using Shared;
namespace ComwellApp.Services.Brugere;

public interface IBrugereService
{
    public Task TilfoejElev(Bruger nyBruger, Bruger ansvarlig);
}