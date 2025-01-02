using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ObjectsExchange;
using ObjectsExchange.Client.Services;
using ObjectsExchange.Components;
using ObjectsExchange.Data;
using ObjectsExchange.Models;
using ObjectsExchange.Services;
using Radzen;
using Sabatex.RadzenBlazor;


var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserConfiguration("sabatex-exchange.json");
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddRazorComponents().AddInteractiveWebAssemblyComponents();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddSabatexRadzenBlazor();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, PersistingServerAuthenticationStateProvider>();
builder.Services.AddSingleton<IEmailSender<IdentityUser>, IdentityNoOpEmailSender>();
builder.Services.AddScoped<ClientManager>();
builder.Services.AddHeaderPropagation(o => o.Headers.Add("Cookie"));
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
                .AddIsConfiguredMicrosoft(builder.Configuration)
                .AddIsConfiguredGoogle(builder.Configuration);
                //.AddIdentityCookies();

builder.Services.AddAuthorization();
builder.Services.AddDbContext<ObjectsExchangeDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ObjectsExchangeDbContext>()
                .AddDefaultTokenProviders();
builder.Services.AddControllers();

builder.Services.AddLocalization(options => { options.ResourcesPath = "Resources"; });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseWebAssemblyDebugging();
}
else
{ 
   app.UseExceptionHandler("/Error");
   app.UseHsts();
}
//app.UseForwardedHeaders(new ForwardedHeadersOptions
//{
//    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
//});

//app.UseHttpsRedirection();
app.UseWhen(context => !context.Request.Path.StartsWithSegments("/api/v0"),
                           builder => builder.UseHttpsRedirection());

//app.UseODataQueryRequest();
app.UseHeaderPropagation();
app.UseStaticFiles();
app.MapControllers();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

var supportedCultures = new[] { "en-US", "uk-UA" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[1])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);
app.UseRequestLocalization(localizationOptions);

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(ObjectsExchange.Client._Imports).Assembly);
app.MapAdditionalIdentityEndpoints();
//await DataSeed.InitializeAsync(app.Services.CreateScope().ServiceProvider,builder.Configuration);

app.Run();



