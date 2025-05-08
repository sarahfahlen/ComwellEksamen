using Shared;

namespace ComwellApp.Services.Elevplan;

public interface IElevplanService
{
    public Task<Shared.Elevplan> OpretElevplan(Bruger ansvarlig);

    //Bruges til at oprette default skabelon til nye elever
    public Task<Shared.Elevplan> LavDefaultSkabelon(Bruger ansvarlig);
}