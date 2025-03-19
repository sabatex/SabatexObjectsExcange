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
using Sabatex.Identity.UI;
using Sabatex.RadzenBlazor;

namespace ObjectsExchange;
public class Program
{
    public static async Task Main(string[] args)
    {

        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.AddUserConfiguration("objectsexchange.json");
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        Sabatex.Blazor.Globalization.AddCulture("uk");

        builder.Services.AddRazorComponents().AddInteractiveWebAssemblyComponents();
        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddSabatexRadzenBlazor();
        builder.Services.AddScoped<ApplicationUserAccessor>();
        builder.Services.AddScoped<IdentityRedirectManager>();
        builder.Services.AddScoped<AuthenticationStateProvider, PersistingServerAuthenticationStateProvider>();
        builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityEmailSender>();
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
        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                        .AddEntityFrameworkStores<ObjectsExchangeDbContext>()
                        .AddDefaultTokenProviders();
        builder.Services.AddControllers();

        builder.Services.AddLocalization(options => { options.ResourcesPath = "Resources"; });
        builder.Services.AddScoped<ICommandLineOperations, CommandLineOperations>();

        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        });




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

        //app.UseHttpsRedirection();
        app.UseWhen(context => !context.Request.Path.StartsWithSegments("/api/v0"),
                                   builder => builder.UseHttpsRedirection());

        app.MapStaticAssets();
        //app.UseODataQueryRequest();
        //app.UseHeaderPropagation();
        //app.UseStaticFiles();

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseAntiforgery();

        app.UseRequestLocalization(
            new RequestLocalizationOptions() { ApplyCurrentCultureToResponseHeaders = true }
            .AddSupportedCultures(new[] { "en-US", "uk-UA" })
            .AddSupportedUICultures(new[] { "en-US", "uk-UA" })
            .SetDefaultCulture("uk-UA")
        );

        app.MapRazorComponents<App>()
            .AddInteractiveWebAssemblyRenderMode()
            .AddAdditionalAssemblies(typeof(Sabatex.Identity.UI._Imports).Assembly, typeof(ObjectsExchange.Client._Imports).Assembly);
        app.MapAdditionalIdentityEndpoints();
        app.MapControllers();
        //await app.RunAsync(args);
        await app.RunAsync(args);



    }
}