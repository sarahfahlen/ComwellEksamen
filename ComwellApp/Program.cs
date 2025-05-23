using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ComwellApp;
using ComwellApp.Services;
using ComwellApp.Services.Brugere;
using ComwellApp.Services.Elevplan;
using ComwellApp.Services.Learning;
using ComwellApp.Services.Login;
using ComwellApp.Services.Lokation;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HttpClient til API-kald (til backend)
builder.Services.AddScoped(sp => new HttpClient
{
   BaseAddress = new Uri("http://localhost:5237")
    //BaseAddress = new Uri("https://apicomwellworkflow.azurewebsites.net")
});

// Lokal storage
builder.Services.AddBlazoredLocalStorage();

// Id-generator (brugt i frontend)
builder.Services.AddScoped<IdGeneratorService>();

builder.Services.AddScoped<IElevplanService, ElevplanServiceServer>();
builder.Services.AddScoped<IBrugereService, BrugereServiceServer>();
builder.Services.AddScoped<ILoginService, LoginServiceServer>();
builder.Services.AddScoped<ILearningService, LearningServiceServer>();
builder.Services.AddScoped<ILokationService, LokationServiceServer>();




// Kør appen
await builder.Build().RunAsync();
