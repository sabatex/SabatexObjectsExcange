using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using ObjectsExchange.Data;
using ObjectsExchange.Models;
using static ObjectsExchange.DataSeed;
namespace ObjectsExchange;

public class DataSeed
{
    //internal class User
    //{
    //    public string? Email { get; set; }
    //    public string? Password { get; set; }
        
    //}
    internal record User (string? Email, string? Password);
    internal record UserInRole(string? Role,string? User);
    internal record UserClient(string OwnerUser,string Description,Guid Id);
    internal record ClientNode(Guid Id,Guid ClientId,string Password,string Name,string? Description,string ClientAccess,bool IsDemo);
    internal record SeedData(string[]? Roles, User[]? Users, UserInRole[]? UsersInRoles, UserClient[]? Clients, ClientNode[]? ClientNodes);
    
    public static async Task InitializeAsync(IServiceProvider serviceProvider,IConfiguration configuration)
    {

        if (!configuration.GetValue<bool>("Migrate"))
            return;

        var context = serviceProvider.GetRequiredService<ObjectsExchangeDbContext>();

        await context.Database.MigrateAsync();


        if (context.Users.Any())
        {
            //return;
        }
        var seedData = new SeedData(null,null,null,null,null);
        configuration.Bind("SeedData", seedData);
 
        using var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var role in seedData.Roles ?? new string[] { })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role) );
            }
        }

        using var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
        foreach (var userData in seedData.Users ?? new User[] {})
        {
            if (userData.Email == null || userData.Password == null) continue;
            if (await userManager.FindByEmailAsync(userData.Email) != null) continue;

            var user = new IdentityUser
            {
                Email = userData.Email,
                NormalizedEmail = userData.Email.ToUpper(),
                UserName = userData.Email,
                NormalizedUserName = userData.Email.ToUpper(),
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString("D")
            };

            var password = new PasswordHasher<IdentityUser>();
            var hashed = password.HashPassword(user, userData.Password);
            user.PasswordHash = hashed;
            context.Users.Add(user);
            await context.SaveChangesAsync();
            
        }
        foreach (var userRole in seedData.UsersInRoles ?? new UserInRole[] { })
        {
            if (userRole.User == null || userRole.Role == null) continue;
 
            var user = await userManager.FindByEmailAsync(userRole.User);
            if (user == null) continue;
            await userManager.AddToRoleAsync(user, userRole.Role);
         }
        foreach (var userClient in seedData.Clients ?? new UserClient[] { })
        {
            if (await context.Clients.AnyAsync(s => s.Id == userClient.Id)) continue;

                var client = new Sabatex.ObjectsExchange.Models.Client();
                client.OwnerUser = userClient.OwnerUser;
                client.Description = userClient.Description;
                client.Id = userClient.Id;
                await context.Clients.AddAsync(client);
                await context.SaveChangesAsync();

         }
        foreach (var clientNode in seedData.ClientNodes ?? new ClientNode[] { })
        {
            if (await context.ClientNodes.AnyAsync(s => s.Id == clientNode.Id)) continue;
            var node = new Sabatex.ObjectsExchange.Models.ClientNode
            {
                Id = clientNode.Id,
                ClientId = clientNode.ClientId,
                IsDemo = clientNode.IsDemo,
                ClientAccess = clientNode.ClientAccess,
                Description = clientNode.Description,
                Password = clientNode.Password,
                Name = clientNode.Name,
                

            };
            await context.AddAsync(node);
            await context.SaveChangesAsync();

        }

    



    }
}
