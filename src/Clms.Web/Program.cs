using Clms.Ui;
using Clms.Ui.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Routes lives in Clms.Ui — the same component a MAUI Blazor Hybrid host would mount.
builder.RootComponents.Add<Routes>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// API base address. Override per environment in wwwroot/appsettings.json.
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5090/";
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

builder.Services.AddScoped<IClmsApi, HttpClmsApi>();

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<ClmsAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<ClmsAuthStateProvider>());

await builder.Build().RunAsync();
