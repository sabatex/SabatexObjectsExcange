using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;
using Sabatex.ObjectsExchange.Client;
using Sabatex.ObjectsExchange.Client.Authentication;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp =>
{
    var authorizationMessageHandler = sp.GetRequiredService<AuthorizationMessageHandler>();
    authorizationMessageHandler.InnerHandler = new HttpClientHandler();
    authorizationMessageHandler = authorizationMessageHandler.ConfigureHandler(
        authorizedUrls: new[] { "https://localhost:7035", "https://sabatex.francecentral.cloudapp.azure.com" },
        scopes: new[] { "" });
    return new HttpClient(authorizationMessageHandler)
    {
#if DEBUG
        BaseAddress = new Uri("https://localhost:7202")
#else
        BaseAddress = new Uri("https://sabatex.francecentral.cloudapp.azure.com")
#endif
    };
});
builder.Services.AddMsalAuthentication<RemoteAuthenticationState, ApplicationUserAccount>(options =>
{
    options.ProviderOptions.Authentication.ClientId = "8fa02a5a-2cf0-4d3e-a818-8ef37dd18f70";
    options.ProviderOptions.Authentication.Authority = "https://login.microsoftonline.com/serhiylakashotmail.onmicrosoft.com";
    options.ProviderOptions.Authentication.ValidateAuthority = true;
    options.ProviderOptions.DefaultAccessTokenScopes.Add("api://5ea93dd0-a7b0-4b16-ac3e-50c07f7f13ef/access_as_user");
    options.UserOptions.RoleClaim = "appRole";

})
      .AddAccountClaimsPrincipalFactory<RemoteAuthenticationState, ApplicationUserAccount, ApplicationUserFactory>();


builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<TooltipService>();
builder.Services.AddScoped<ContextMenuService>();

await builder.Build().RunAsync();
