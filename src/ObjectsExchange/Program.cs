namespace ObjectsExchange;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using ObjectsExchange.Data;
using Westwind.AspNetCore.Markdown;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<ObjectsExchangeDbContext>(options =>
                    options.UseSqlite(builder.Configuration.GetConnectionString("SqliteConnection")));

        builder.Services.Configure<ObjectsExchange.Services.ApiConfig>(
                   builder.Configuration.GetSection(nameof(ObjectsExchange.Services.ApiConfig)));


        // Add services to the container.
        builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

        builder.Services.AddAuthorization(options =>
        {
            // By default, all incoming requests will be authorized according to the default policy.
            // options.FallbackPolicy = options.DefaultPolicy;
        });
        builder.Services.AddMarkdown();

        builder.Services.AddRazorPages().AddMicrosoftIdentityUI().AddApplicationPart(typeof(MarkdownPageProcessorMiddleware).Assembly);
        builder.Services.AddScoped<ObjectsExchange.Services.ClientManager>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
            app.UseHttpsRedirection();
        }

        app.UseMarkdown();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapRazorPages();
        app.MapControllers();

        app.Run();

    }
}
