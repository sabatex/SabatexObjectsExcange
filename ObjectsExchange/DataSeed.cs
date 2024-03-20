using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using ObjectsExchange.Data;
using ObjectsExchange.Models;
namespace ObjectsExchange;

public class DataSeed
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {

        var context = serviceProvider.GetRequiredService<ObjectsExchangeDbContext>();

        if (context.Users.Any())
        {
            return;
        }

        string[] roles = ["Administrator", "Client"];
        using var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new ApplicationRole(role));
            }
        }

        using var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        foreach (var role in roles)
        {
            var user = new ApplicationUser
            {
                Email = $"{role}@contoso.com",
                NormalizedEmail = $"{role.ToUpper()}@CONTOSO.COM",
                UserName = $"{role}@contoso.com",
                NormalizedUserName = $"{role.ToUpper()}@CONTOSO.COM",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString("D")
            };

            var password = new PasswordHasher<ApplicationUser>();
            var hashed = password.HashPassword(user, "Passw0rd!");
            user.PasswordHash = hashed;
            context.Users.Add(user);
            await context.SaveChangesAsync();
            await userManager.AddToRoleAsync(user, role);
        }
    }
}
