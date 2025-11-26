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
using Org.BouncyCastle.Tls;
using Radzen;
using Sabatex.Core.Identity;
using Sabatex.Core.RadzenBlazor;
using Sabatex.ObjectsExchange.Models;
using Sabatex.RadzenBlazor;
using Sabatex.RadzenBlazor.Server;



        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.AddUserConfiguration("objectsexchange.json");
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        builder.Services.AddRazorComponents().AddInteractiveWebAssemblyComponents();
        builder.Services.AddCascadingAuthenticationState();
        
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = IdentityConstants.ApplicationScheme;
            options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
        })
                        .AddIsConfiguredMicrosoft(builder.Configuration)
                        .AddIsConfiguredGoogle(builder.Configuration);
        builder.Services.AddAuthorization();
        
        builder.Services.AddDbContext<ObjectsExchangeDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();
        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                        .AddEntityFrameworkStores<ObjectsExchangeDbContext>()
                        .AddRoles<IdentityRole>()
                        .AddSignInManager()
                        .AddDefaultTokenProviders();
        
        builder.Services.AddLocalization(options => { options.ResourcesPath = "Resources"; });
        builder.Services.AddControllers();
        builder.Services.AddSabatexRadzenBlazor();
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        });

        builder.Services.AddScoped<AuthenticationStateProvider, PersistingServerAuthenticationStateProvider>();
        builder.Services.AddSingleton<Sabatex.Core.Identity.IEmailSender<ApplicationUser>, IdentityEmailSender>();
        builder.Services.AddScoped<ISabatexRadzenBlazorDataAdapter, ServerDataAdapter>();
        builder.Services.AddScoped<IIdentityAdapter, IdentityAdapterServer>();
        //Sabatex.Blazor.Globalization.AddCulture("uk");
        //builder.Services.AddScoped<ApplicationUserAccessor>();
        //builder.Services.AddScoped<IdentityRedirectManager>();
        //builder.Services.AddScoped<ClientManager>();
        //builder.Services.AddHeaderPropagation(o => o.Headers.Add("Cookie"));
        //.AddIdentityCookies();
        //

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
            .AddAdditionalAssemblies(typeof(ObjectsExchange.Client._Imports).Assembly, typeof(Sabatex.RadzenBlazor._Imports).Assembly);
        app.MapAdditionalIdentityEndpoints();
        app.MapControllers();

        await app.RunAsync(args,async () =>
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            await roleManager.GetOrCreateRoleAsync(UserRole.Administrator);
            await roleManager.GetOrCreateRoleAsync(UserRole.Client);
            await roleManager.GetOrCreateRoleAsync(UserRole.ClientUser);

            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var dbContext = services.GetRequiredService<ObjectsExchangeDbContext>();
            foreach (var userData in demoUsers())
            {
                var user = userManager.GetOrCreateUserAsync(userData.Email, userData.Password, userData.Role);

                foreach (var client in userData.DemoClients)
                {
                    var c = await dbContext.Clients.FirstOrDefaultAsync(s => s.Id == client.Id);
                    if (c == null)
                    {
                        await dbContext.Clients.AddAsync(new Sabatex.ObjectsExchange.Models.Client
                        {
                            OwnerUser = userData.Email,
                            Description = client.Description,
                            Id = client.Id
                        });
                        await dbContext.SaveChangesAsync();
                        foreach (var node in client.DemoNodes)
                        {
                            var n = await dbContext.ClientNodes.FirstOrDefaultAsync(s => s.Id == node.Id);
                            if (n == null)
                            {
                                await dbContext.ClientNodes.AddAsync(new Sabatex.ObjectsExchange.Models.ClientNode
                                {
                                    Description = node.Description,
                                    Id = node.Id,
                                    ClientId = client.Id,
                                    IsDemo = node.IsDemo,
                                    Name = node.Name,
                                    Password = node.Password
                                });
                                await dbContext.SaveChangesAsync();
                            }
                        }
                    }
                }
            }


        }, async (userName) =>
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            await userManager.GrandUserAdminRoleAsync(userName);

        }, async () =>
        {
            // Migrate database
            try
            {
                using var scope = app.Services.CreateScope();
                var services = scope.ServiceProvider;
                var dbContext = services.GetRequiredService<ObjectsExchangeDbContext>();
                await dbContext.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                var logger = app.Services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while migrating the database.");
            }
        }
        );
DemoUser[] demoUsers() => new DemoUser[]
    {
        new DemoUser("DemoUser@contoso.com","DemoPassword1!",UserRole.Client,new DemoClient[]
        {
            new DemoClient(new Guid("C4F63E3F-D19E-4019-AF88-6071AB01E1D2"),"DemoClient",new DemoNode[]
            {
                new DemoNode(new Guid("161BAB4F-02DE-4616-B3DD-826EC8288026"),"FirstDemoNode","Demonstration node for demonstration user","DemoApiPassword",true),
                new DemoNode(new Guid("6E5EBE6B-120B-4D6C-8EE1-6EA0260FFC91"),"SecondDemoNode","Demonstration node for demonstration user","DemoApiPassword",true)
            })
        })
    };
record DemoClient(Guid Id, string Description, DemoNode[] DemoNodes);
record DemoNode(Guid Id, string Name, string Description, string Password, bool IsDemo);

record DemoUser(string Email, string Password, string Role, DemoClient[] DemoClients);

