using Microsoft.EntityFrameworkCore;
using Sabatex.ObjectsExchange.Server.Data;
using Sabatex.ObjectsExchange.Server.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Identity.Web;
using Microsoft.Net.Http.Headers;
using Microsoft.Identity.Web.UI;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<Sabatex.ObjectsExchange.Services.ApiConfig>(
           builder.Configuration.GetSection(nameof(Sabatex.ObjectsExchange.Services.ApiConfig)));

builder.Services.AddDbContext<ObjectsExchangeDbContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("SqliteConnection")));


builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));
builder.Services.AddAuthorization(options =>
{
    // By default, all incoming requests will be authorized according to the default policy.
    // options.FallbackPolicy = options.DefaultPolicy;
});

builder.Services.AddControllers();
builder.Services.AddRazorPages()
    .AddMicrosoftIdentityUI();


//builder.Services.AddCors(options =>
//        {
//            options.AddDefaultPolicy(
//                policy =>
//                {
//                    policy.WithOrigins("https://localhost:7035", "https://sabatex.francecentral.cloudapp.azure.com")
//                    //.AllowAnyHeader()
//                    .AllowAnyMethod()
//                    .WithHeaders(HeaderNames.ContentType, HeaderNames.Authorization, "x-custom-header")
//                    .AllowCredentials();
//                });
//        });
// Add services to the container.


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();

builder.Services.AddScoped<ClientManager>();


var app = builder.Build();

// Configure the HTTP request pipeline.

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();


//app.UseCors();

app.UseAuthorization();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.Run();
