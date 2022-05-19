using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApiDocumentsExchange.Models;

namespace WebApiDocumentsExchange.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public DbSet<ClientNode> ClientNodes { get; set; }
    public DbSet<ObjectExchange> ObjectExchanges { get; set; }
    public DbSet<QueryObject> QueryObjects { get; set; }

    public DbSet<AutenficatedNode> AutenficatedNodes { get; set; }


    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        var adminRole = new IdentityRole
        {
            Id = "ce38850f-080a-451e-9abd-d233845ccf89",
            Name = "Admin",
            NormalizedName = "ADMIN",
            ConcurrencyStamp = "66174ed2-7cce-4d85-a668-7cba67f84818"
        };

        var adminRoleClaim = new IdentityRoleClaim<string>
        {
            RoleId = adminRole.Id,
            ClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name",
            ClaimValue = "Admin"
        };

        var adminUser = new IdentityUser
        {
            Id = "e886692e-26cd-4731-89fe-997f06b715cf",
            UserName = "admin@sabatex.github",
            NormalizedUserName = "ADMIN@SABATEX.GITHUB",
            Email = "admin@sabatex.github",
            NormalizedEmail = "ADMIN@SABATEX.GITHUB",
            EmailConfirmed = true,
            PasswordHash = "AQAAAAEAACcQAAAAEPRL0LeJfSOGoyLArpPkBFHqujEy6/WiHBk4/qMkjgTuNyTgUYxGsVzAeg//F+dwxw==",
            SecurityStamp = "CTDABLX7JKXS7FC5WUZXTISX2IKZO7ZA",
            ConcurrencyStamp= "72089f6c-c148-4952-9b7d-73cd0ccb6f54",
            PhoneNumberConfirmed = false,
            TwoFactorEnabled = false,
            LockoutEnabled = true
        };

        //builder.Entity<IdentityUser>().HasData(adminUser);
        //builder.Entity<IdentityRole>().HasData(adminRole);
        

        //builder.Entity<ObjectExchange>().HasKey(k => new { k.Id, k.SenderId, k.DestinationId });
        //builder.Entity<Object1C>(en =>
        //{
        //    en.Property(p => p.DateStamp).HasColumnType("timestamp without time zone");
        //});
        //builder.Entity<UnresolvedObject>(en =>
        //{
        //    en.Property(p => p.DateStamp).HasColumnType("timestamp without time zone");
        //});
    }
}
