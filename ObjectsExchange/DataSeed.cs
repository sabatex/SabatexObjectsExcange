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
    internal record SeedData(string[]? Roles, User[]? Users, UserInRole[]? UsersInRoles);
    public static async Task InitializeAsync(IServiceProvider serviceProvider,IConfiguration configuration)
    {

        var context = serviceProvider.GetRequiredService<ObjectsExchangeDbContext>();

        await context.Database.MigrateAsync();


        if (context.Users.Any())
        {
            //return;
        }
        var seedData = new SeedData(null,null,null);
        configuration.Bind("SeedData", seedData);
 
        using var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        foreach (var role in seedData.Roles ?? new string[] { })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new ApplicationRole(role));
            }
        }

        using var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        foreach (var userData in seedData.Users ?? new User[] {})
        {
            if (userData.Email == null || userData.Password == null) continue;
            if (await userManager.FindByEmailAsync(userData.Email) != null) continue;

            var user = new ApplicationUser
            {
                Email = userData.Email,
                NormalizedEmail = userData.Email.ToUpper(),
                UserName = userData.Email,
                NormalizedUserName = userData.Email.ToUpper(),
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString("D")
            };

            var password = new PasswordHasher<ApplicationUser>();
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
            
    }
}
