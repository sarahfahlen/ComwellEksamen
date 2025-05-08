using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ComwellApp;
using ComwellApp.Services;
using ComwellApp.Services.Brugere;
using ComwellApp.Services.Elevplan;
using ComwellApp.Services.Login;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddBlazoredLocalStorage();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<ElevplanServiceMock>();
builder.Services.AddScoped<IdGeneratorService>();
builder.Services.AddScoped<ILoginService, LoginServiceClientSite>();

// 👇 Factory med async init
builder.Services.AddScoped<IBrugereService>(sp =>
{
    var elevplanService = sp.GetRequiredService<ElevplanServiceMock>();
    var idGenerator = sp.GetRequiredService<IdGeneratorService>();
    var service = new BrugereServiceMock(elevplanService, idGenerator);

    // Bemærk: vi _kan ikke await_ InitAsync her direkte (kun i Blazor Server)
    // Så enten drop InitAsync helt, eller kald den manuelt i første komponent

    return service;
});

// byg og kør
await builder.Build().RunAsync();