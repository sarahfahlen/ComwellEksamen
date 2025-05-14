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

// HttpClient til API-kald (til backend)
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("http://localhost:5237")
});

// Lokal storage
builder.Services.AddBlazoredLocalStorage();

// Id-generator (brugt i frontend)
builder.Services.AddScoped<IdGeneratorService>();

builder.Services.AddScoped<IElevplanService, ElevplanServiceServer>();
builder.Services.AddScoped<IBrugereService, BrugereServiceServer>();
builder.Services.AddScoped<ILoginService, LoginServiceServer>();


// Kør appen
await builder.Build().RunAsync();