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
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri("http://localhost:5237") 
});
// Lokal storage
builder.Services.AddBlazoredLocalStorage();

// HttpClient til API-kald (lokal base-adresse)
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Registrér services
builder.Services.AddScoped<ElevplanServiceMock>();
builder.Services.AddScoped<IdGeneratorService>();

builder.Services.AddScoped<IElevplanService>(sp =>
    sp.GetRequiredService<ElevplanServiceMock>());


// 👇 Registrér BrugereServiceMock FØR loginservice, så Emil tilføjes i tide
builder.Services.AddScoped<IBrugereService>(sp =>
{
    var elevplanService = sp.GetRequiredService<ElevplanServiceMock>();
    var idGenerator = sp.GetRequiredService<IdGeneratorService>();
    var service = new BrugereServiceMock(elevplanService, idGenerator);
    return service;
});

// LoginService skal tilgå listen EFTER BrugereServiceMock er oprettet
builder.Services.AddScoped<ILoginService, LoginServiceClientSite>();

// Byg og kør app
await builder.Build().RunAsync();