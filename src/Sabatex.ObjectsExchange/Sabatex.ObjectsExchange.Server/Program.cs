using Microsoft.EntityFrameworkCore;
using Sabatex.ObjectsExchange.Server.Data;
using Sabatex.ObjectsExchange.Server.Services;

namespace Sabatex.ApiObjectsExchange
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.Configure<Sabatex.ObjectsExchange.Services.ApiConfig>(
               builder.Configuration.GetSection(nameof(Sabatex.ObjectsExchange.Services.ApiConfig)));
            builder.Services.AddDbContext<ObjectsExchangeDbContext>(options =>
                options.UseSqlite(builder.Configuration.GetConnectionString("SqliteConnection")));
            builder.Services.AddScoped<ClientManager>();

            // Add services to the container.

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
        }
    }
}