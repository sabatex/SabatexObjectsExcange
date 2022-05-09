using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Radzen;
using WebApiDocumentsExchange.Data;
using WebApiDocumentsExchange.Areas.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var timeSpan = builder.Configuration.GetValue<int>("TokenValid", 15);
WebApiDocumentsExchange.Extensions.ExcangeExtensions.TokenValid = TimeSpan.FromMinutes(timeSpan);



builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var configuration = builder.Configuration;
    var provider = configuration.GetValue("Provider", "Sqlite");
    switch (provider.ToLower())
    {
        case "sqlite":
            options.UseSqlite(configuration.GetConnectionString("SqliteConnection"),
                 x => x.MigrationsAssembly("SqliteMigrations"));
            break;
        case "sqlserver":
            options.UseSqlServer(
                                configuration.GetConnectionString("SqlServerConnection"),
                                x => x.MigrationsAssembly("SqlServerMigrations"));
            break;
        case "postgresql":
            options.UseNpgsql(
                                configuration.GetConnectionString("PostgreSqlConnection"),
                                x => x.MigrationsAssembly("PostgreSQLMigrations"));

            break;
        default: throw new Exception($"Unsupported provider: {provider}");
    }
});
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();


builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>();

builder.Services.AddScoped<ContextMenuService>();
builder.Services.AddScoped<DialogService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapBlazorHub();

app.MapFallbackToPage("/_Host");

await CreateDefaultRoles(app);

app.Run();

async Task CreateDefaultRoles(WebApplication app)
{
    using (var serviceScope = app.Services.CreateScope())
    {
        var services = serviceScope.ServiceProvider;
        var RoleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var UserManager = services.GetRequiredService<UserManager<IdentityUser>>();

        var adminRole = await RoleManager.FindByNameAsync("Admin");

        if (adminRole == null)
        {
            IdentityResult adminRoleResul = await RoleManager.CreateAsync(new IdentityRole("Admin"));
            adminRole = await RoleManager.FindByNameAsync("Admin");
            var claimAdminRole = await RoleManager.AddClaimAsync(adminRole, new Claim(ClaimTypes.Name, "Admin"));
        }


        //var claimAdminRole = await RoleManager.AddClaimAsync(adminRole, new Claim(ClaimTypes.Name,"Admin"));
        var userToMakeAdmin = await UserManager.FindByNameAsync("serhiy.lakas@hotmail.com");
        if (userToMakeAdmin != null)
            await UserManager.AddToRoleAsync(userToMakeAdmin, "Admin");

    }
}

public partial class Program { }