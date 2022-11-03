using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Radzen;
using WebApiDocumentsExchange.Data;
using WebApiDocumentsExchange.Areas.Identity;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<WebApiDocumentsExchange.Services.ApiConfig>(
    builder.Configuration.GetSection(nameof(WebApiDocumentsExchange.Services.ApiConfig)));

var dbProvider = builder.Configuration.GetValue("DataBaseProvider", "postgres").ToLower();
switch (dbProvider)
{
    case "sqlite":
        builder.Services.AddDbContext<ExchangeDbContext, SQLiteDbContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("SqliteConnection")));
        break;
    case "mssql":
        builder.Services.AddDbContext<ExchangeDbContext, MSSQLDbContext>(options =>
             options.UseSqlServer(builder.Configuration.GetConnectionString("MSSQLConnection")));
        break;
    case "postgres":
        builder.Services.AddDbContext<ExchangeDbContext, PostgresDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection")));
        break;
    default: throw new Exception($"Unsupported provider: {dbProvider}");
}

builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ExchangeDbContext>();


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
    //app.UseHsts();

    //temporary
    app.UseSwagger();
    app.UseSwaggerUI();

}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapBlazorHub();

app.MapFallbackToPage("/_Host");

if (args.Length > 0)
{
    foreach (var arg in args)
    {
        switch (arg.ToLower())
        {
            case "initialize":
                await CreateDefaultRoles(app);
                break;
        }

    }

}



app.Run();

async Task CreateDefaultRoles(WebApplication app)
{
    using (var serviceScope = app.Services.CreateScope())
    {
        var services = serviceScope.ServiceProvider;
        var RoleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var UserManager = services.GetRequiredService<UserManager<IdentityUser>>();
        
        // Admin
        var adminRole = await RoleManager.FindByNameAsync("Admin");
        if (adminRole == null)
        {
            var result = await RoleManager.CreateAsync(new IdentityRole("Admin"));
            if (result.Succeeded == false)
                throw new Exception("Error! Do not create Admin role!");
            adminRole = await RoleManager.FindByNameAsync("Admin");
        }
        
        //var claimRole = new Claim(ClaimTypes.Name, "Admin");
        //var claimsRole = await RoleManager.GetClaimsAsync(adminRole);
        //if (!claimsRole.Contains(claimRole))
        //{
        //    var result = await RoleManager.AddClaimAsync(adminRole, claimRole);
        //    if (result.Succeeded == false)
        //        throw new Exception("Error! Do not create Claim for Admin role!");
        //}
        
        var userToMakeAdmin = await UserManager.FindByNameAsync("admin@sabatex.github");
        if (userToMakeAdmin == null)
        {
            var adminUser = new IdentityUser
            {
                UserName = "admin@sabatex.github",
                NormalizedUserName = "ADMIN@SABATEX.GITHUB",
                Email = "admin@sabatex.github",
                NormalizedEmail = "ADMIN@SABATEX.GITHUB",
                EmailConfirmed = true,
                PasswordHash = "AQAAAAEAACcQAAAAEPRL0LeJfSOGoyLArpPkBFHqujEy6/WiHBk4/qMkjgTuNyTgUYxGsVzAeg//F+dwxw==",
                SecurityStamp = "CTDABLX7JKXS7FC5WUZXTISX2IKZO7ZA",
                ConcurrencyStamp = "72089f6c-c148-4952-9b7d-73cd0ccb6f54",
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = true
            };
            var result = await UserManager.CreateAsync(adminUser);
            if (result.Succeeded == false)
                throw new Exception("Error! Do not create  Admin user!");
            userToMakeAdmin = await UserManager.FindByNameAsync("admin@sabatex.github");
        }
        
        if (! await UserManager.IsInRoleAsync(userToMakeAdmin, "Admin"))
        {
            await UserManager.AddToRoleAsync(userToMakeAdmin, "Admin");
        }
        
       

        //User
        var userRole = await RoleManager.FindByNameAsync("User");
        if (userRole == null)
        {
            var result = await RoleManager.CreateAsync(new IdentityRole("User"));
            if (result.Succeeded == false)
                throw new Exception("Error! Do not create User role!");
            userRole = await RoleManager.FindByNameAsync("User");
        }
        
        //claimRole = new Claim(ClaimTypes.Name, "User");
        //claimsRole = await RoleManager.GetClaimsAsync(userRole);
        //if (!claimsRole.Contains(claimRole))
        //{
        //    var result = await RoleManager.AddClaimAsync(userRole, claimRole);
        //    if (result.Succeeded == false)
        //        throw new Exception("Error! Do not create Claim for User role!");
        //}

        var userToMakeUser = await UserManager.FindByNameAsync("user@sabatex.github");
        if (userToMakeUser == null)
        {
            var user = new IdentityUser
            {
                UserName = "user@sabatex.github",
                NormalizedUserName = "USER@SABATEX.GITHUB",
                Email = "user@sabatex.github",
                NormalizedEmail = "USER@SABATEX.GITHUB",
                EmailConfirmed = true,
                PasswordHash = "AQAAAAEAACcQAAAAEJBMqdDHkZPi+zd0pliccS+L9e86ZTbJdXm/5aZ4tepcZZSTO3k6RkkH+uz0xR7uXw==",
                SecurityStamp = "DHWIYQLGJHBOHBSN6GFCGA6UIKAXF7QL",
                ConcurrencyStamp = "05072c87-6375-4b1b-96f5-827e16d9bebc",
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = true
            };
            var result = await UserManager.CreateAsync(user);
            if (result.Succeeded == false)
                throw new Exception("Error! Do not create  User!");
            userToMakeUser = await UserManager.FindByNameAsync("user@sabatex.github");
        }

        if (!await UserManager.IsInRoleAsync(userToMakeUser, "User"))
        {

            await UserManager.AddToRoleAsync(userToMakeUser, "User");
        }

        var dbContext = serviceScope.ServiceProvider.GetRequiredService<ExchangeDbContext>();
        string sendertest = "SenderTest";
        string destinationtest = "DestinationTest";
        var st = await dbContext.ClientNodes.FindAsync(sendertest.ToLower());
        if (st == null)
        {
            await dbContext.ClientNodes.AddAsync(new WebApiDocumentsExchange.Models.ClientNode
            {
                Name = sendertest,
                Id = sendertest.ToLower(),
                Description = "sender test (For testing)",
                Password = "1"

            });
            await dbContext.SaveChangesAsync();

        }

        var dt = await dbContext.ClientNodes.FindAsync(destinationtest.ToLower());
        if (dt == null)
        {
            await dbContext.ClientNodes.AddAsync(new WebApiDocumentsExchange.Models.ClientNode
            {
                Id=destinationtest.ToLower(),
                Name = destinationtest,
                Description = "destination test (For testing)",
                Password = "1"

            });
            await dbContext.SaveChangesAsync();

        }


    }
}

public partial class Program { }