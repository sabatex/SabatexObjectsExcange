using Microsoft.EntityFrameworkCore;
using WebApi1C8Exchange.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var configuration = builder.Configuration;
    var provider = configuration.GetValue("Provider", "Sqllite");
    switch (provider.ToLower())
    {
        case "sqlite":
            options.UseSqlite(configuration.GetConnectionString("SqliteConnection"),
                 x => x.MigrationsAssembly("MigrationsSqlite"));
            break;
        case "sqlserver":
            options.UseSqlServer(
                                configuration.GetConnectionString("SqlServerConnection"),
                                x => x.MigrationsAssembly("SqlServerMigrations"));
            break;
        case "potgresql":
            options.UseSqlServer(
                                configuration.GetConnectionString("PostgreSqlConnection"),
                                x => x.MigrationsAssembly("PostgreSQLMigrations"));

            break;
        default: throw new Exception($"Unsupported provider: {provider}");
    }



});


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
