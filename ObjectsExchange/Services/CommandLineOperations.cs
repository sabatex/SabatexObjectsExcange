using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ObjectsExchange.Data;
using Sabatex.Identity.UI;
using Sabatex.ObjectsExchange.Models;

namespace ObjectsExchange.Services;

public class CommandLineOperations: ICommandLineOperations
{
    record DemoClient(Guid Id, string Description, DemoNode[] DemoNodes);
    record DemoNode(Guid Id, string Name, string Description, string Password, bool IsDemo);
    record DemoUser(string Email, string Password, string Role, DemoClient[] DemoClients);

    DemoUser[] demoUsers => new DemoUser[]
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



    readonly RoleManager<IdentityRole> roleManager;
    readonly UserManager<ApplicationUser> userManager;
    readonly ObjectsExchangeDbContext dbContext;

    public CommandLineOperations(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, ObjectsExchangeDbContext dbContext)
    {
        this.roleManager = roleManager;
        this.userManager = userManager;
        this.dbContext = dbContext;
    }
    public async Task GrandUserAdminRoleAsync(string userName)
    {
        await InitialRolesAsync();
        await userManager.GrandUserAdminRoleAsync(userName);
    }

    public async Task InitialDemoDataAsync()
    {
        await InitialRolesAsync();
        foreach (var userData in demoUsers)
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

    }
    public async Task InitialRolesAsync()
    {
        await roleManager.GetOrCreateRoleAsync(UserRole.Administrator);
        await roleManager.GetOrCreateRoleAsync(UserRole.Client);
        await roleManager.GetOrCreateRoleAsync(UserRole.ClientUser);
    }
    public async Task MigrateAsync()
    {
        await dbContext.Database.MigrateAsync();
        await InitialRolesAsync();
    }
}

