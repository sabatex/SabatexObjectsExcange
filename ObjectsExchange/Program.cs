using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ObjectsExchange;
using ObjectsExchange.Client.Services;
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
builder.Services.AddScoped<ISabatexRadzenBlazorDataAdapter<Guid>, ObjectsExchange.Services.ApiAdapter>();
builder.Services.AddScoped<IApiAdapter, ObjectsExchange.Services.ApiAdapter>();
builder.Services.AddSingleton<SabatexBlazorAppState>();
builder.Services.AddScoped<SabatexJsInterop>();

builder.Services.AddScoped<ClientManager>();
builder.Services.AddHttpClient();
builder.Services.AddHeaderPropagation(o => o.Headers.Add("Cookie"));
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddDbContext<ObjectsExchangeDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("SqlIteConnection"));
//#if DEBUG
//    //options.UseNpgsql(builder.Configuration.GetConnectionString("LocalConnection"));
//    options.UseSqlite(builder.Configuration.GetConnectionString("SqlIteConnection"));
//#else
//    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
//#endif
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


//builder.Services.Configure<ForwardedHeadersOptions>(options =>
//        {
//            options.ForwardedHeaders =
//                ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
//        });

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
//app.UseForwardedHeaders(new ForwardedHeadersOptions
//{
//    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
//});

//app.UseHttpsRedirection();
app.UseWhen(context => !context.Request.Path.StartsWithSegments("/api/v0"),
                           builder => builder.UseHttpsRedirection());
app.MapControllers();
//app.UseODataQueryRequest();
app.UseHeaderPropagation();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();


app.MapRazorComponents<App>().AddInteractiveWebAssemblyRenderMode().AddAdditionalAssemblies(typeof(ObjectsExchange.Client._Imports).Assembly);
app.MapAdditionalIdentityEndpoints();
await DataSeed.InitializeAsync(app.Services.CreateScope().ServiceProvider,builder.Configuration);

app.Run();



