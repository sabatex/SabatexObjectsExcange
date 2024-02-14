using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ObjectsExchange.Data;
namespace ObjectsExchange;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ObjectsExchange.Data;
using Westwind.AspNetCore.Markdown;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found."); ;


        builder.Services.AddDbContext<ObjectsExchangeDbContext>(options =>
                    options.UseNpgsql(connectionString));

        builder.Services.AddDatabaseDeveloperPageExceptionFilter();
        builder.Services.AddLocalization();

        builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddEntityFrameworkStores<ObjectsExchangeDbContext>();

        builder.Services.Configure<ObjectsExchange.Services.ApiConfig>(
                   builder.Configuration.GetSection(nameof(ObjectsExchange.Services.ApiConfig)));

        // add Microsoft account
        var autenficationBuilder = builder.Services.AddAuthentication();

        var microsoftClientId = builder.Configuration["Authentication:Microsoft:ClientId"];
        var microsoftClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"];
        if (!(string.IsNullOrWhiteSpace(microsoftClientId) || string.IsNullOrWhiteSpace(microsoftClientSecret)))
        {
            autenficationBuilder.AddMicrosoftAccount(microsoftOptions =>
            {
                microsoftOptions.ClientId = microsoftClientId;
                microsoftOptions.ClientSecret = microsoftClientSecret;
            });
        }


        builder.Services.AddAuthorization(options =>
        {
            // By default, all incoming requests will be authorized according to the default policy.
            // options.FallbackPolicy = options.DefaultPolicy;
        });
        builder.Services.AddMarkdown();

        builder.Services.AddRazorPages()
                        .AddViewLocalization()
                        .AddDataAnnotationsLocalization()
                        .AddApplicationPart(typeof(MarkdownPageProcessorMiddleware).Assembly);
        builder.Services.AddScoped<ObjectsExchange.Services.ClientManager>();

        builder.Services.AddAuthentication();
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
            app.UseHttpsRedirection();
        }

        app.UseHttpsRedirection();
        app.UseMarkdown();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseRequestLocalization(
            new RequestLocalizationOptions() { ApplyCurrentCultureToResponseHeaders = true }
                .AddSupportedCultures(new[] { "en-US", "uk-UA" })
                .AddSupportedUICultures(new[] { "en-US", "uk-UA" })
                .SetDefaultCulture("uk-UA")
                );

        app.MapRazorPages();
        app.MapControllers();

        app.Run();

    }
}
