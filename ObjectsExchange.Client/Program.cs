using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Security.Claims;
using ObjectsExchange.Client;
using Radzen;
using Radzen.Blazor;
using Sabatex.RadzenBlazor;
using ObjectsExchange.Client.Services;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using System.Globalization;



var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddRadzenComponents();
builder.Services.AddScoped<ISabatexRadzenBlazorDataAdapter<Guid>, ApiAdapter>();
builder.Services.AddScoped<IApiAdapter, ApiAdapter>();
builder.Services.AddSingleton<SabatexBlazorAppState>();
builder.Services.AddScoped<SabatexJsInterop>();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)});
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddSingleton<AuthenticationStateProvider, PersistentAuthenticationStateProvider>();


builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
var host = builder.Build();
var js = host.Services.GetRequiredService<IJSRuntime>();
var culture = await js.InvokeAsync<string>("blazorCulture.get") ?? "uk-UA";
var cultureInfo = new CultureInfo(culture);
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
// set base address for default host


await host.RunAsync();
