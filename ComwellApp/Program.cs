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
builder.Services.AddScoped<IdGeneratorService>();
builder.Services.AddScoped<ILoginService, LoginServiceClientSite>();
builder.Services.AddScoped<IElevplanService, ElevplanServiceMock>();
builder.Services.AddScoped<IBrugereService, BrugereServiceMock>();

await builder.Build().RunAsync();