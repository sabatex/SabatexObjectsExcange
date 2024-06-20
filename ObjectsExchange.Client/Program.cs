using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Security.Claims;
using ObjectsExchange.Client;
using Radzen;
using Radzen.Blazor;
using Sabatex.RadzenBlazor;
using ObjectsExchange.Client.Services;



var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddRadzenComponents();
builder.Services.AddScoped<ISabatexRadzenBlazorDataAdapter<Guid>, ApiAdapter>();
builder.Services.AddScoped<IApiAdapter, ApiAdapter>();
builder.Services.AddSingleton<SabatexBlazorAppState>();
builder.Services.AddScoped<SabatexJsInterop>();


// set base address for default host
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)});
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddSingleton<AuthenticationStateProvider, PersistentAuthenticationStateProvider>();



await builder.Build().RunAsync();
