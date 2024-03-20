using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ObjectsExchange;
using ObjectsExchange.Components;
using ObjectsExchange.Components.Account;
using ObjectsExchange.Data;
using ObjectsExchange.Models;
using ObjectsExchange.Services;
using Radzen;
using Sabatex.RadzenBlazor;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveWebAssemblyComponents();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, PersistingServerAuthenticationStateProvider>();

builder.Services.AddRadzenComponents();
builder.Services.AddScoped<ISabatexRadzenBlazorDataAdapter<Guid>, SabatexServerRadzenBlazorODataAdapter<Guid>>();
builder.Services.AddSingleton<SabatexBlazorAppState>();
builder.Services.AddScoped<SabatexJsInterop>();
//builder.Services.AddControllers();
//builder.Services.AddSabatexRadzenBlazor<SabatexServerRadzenBlazorODataAdapter<Guid>, Guid>();
builder.Services.AddHttpClient();
//builder.Services.AddScoped<RadzenDemo.Server.SabatexExchangeService>();
//builder.Services.AddScoped<RadzenDemo.Client.SabatexExchangeService>();
//builder.Services.AddHttpClient("RadzenDemo.Server").ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { UseCookies = false }).AddHeaderPropagation(o => o.Headers.Add("Cookie"));
builder.Services.AddHeaderPropagation(o => o.Headers.Add("Cookie"));
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
//builder.Services.AddScoped< ObjectsExchange.Client.Services.SecurityService >();
builder.Services.AddDbContext<ObjectsExchangeDbContext>(options =>
{
#if DEBUG
    options.UseNpgsql(builder.Configuration.GetConnectionString("LocalConnection"));
#else
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
#endif
});
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>().AddEntityFrameworkStores<ObjectsExchangeDbContext>().AddDefaultTokenProviders();
builder.Services.AddControllers();
//.AddOData(o =>
//{
//    var oDataBuilder = new ODataConventionModelBuilder();
//    //oDataBuilder.EntitySet<ApplicationUser>("ApplicationUsers");
//    //var usersType = oDataBuilder.StructuralTypes.First(x => x.ClrType == typeof(ApplicationUser));
//    //usersType.AddProperty(typeof(ApplicationUser).GetProperty(nameof(ApplicationUser.Password)));
//    //usersType.AddProperty(typeof(ApplicationUser).GetProperty(nameof(ApplicationUser.ConfirmPassword)));
//    //oDataBuilder.EntitySet<ApplicationRole>("ApplicationRoles");
//    oDataBuilder.EntitySet<ObjectsExchange.Client.Models.Client>("Client");
//    oDataBuilder.EntitySet<ObjectsExchange.Client.Models.ClientNode>("ClientNode");
//    o.AddRouteComponents("odata", oDataBuilder.GetEdmModel()).Count().Filter().OrderBy().Expand().Select().SetMaxTop(null).TimeZone = TimeZoneInfo.Utc;
//});


builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        });

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();



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
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseHttpsRedirection();
app.MapControllers();
//app.UseODataQueryRequest();
app.UseHeaderPropagation();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();


app.MapRazorComponents<App>().AddInteractiveWebAssemblyRenderMode().AddAdditionalAssemblies(typeof(ObjectsExchange.Client._Imports).Assembly);
app.MapAdditionalIdentityEndpoints();
await DataSeed.InitializeAsync(app.Services.CreateScope().ServiceProvider);
app.Run();



