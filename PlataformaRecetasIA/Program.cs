using Microsoft.EntityFrameworkCore;
using PlataformaRecetasIA.Data;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Agregar servicios MVC
builder.Services.AddControllersWithViews();

// Configurar logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
    logging.SetMinimumLevel(LogLevel.Information);
});

// Configurar DbContext para MySQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0))));

// Configurar autenticación JWT
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"];
if (string.IsNullOrEmpty(jwtSecretKey))
{
    throw new InvalidOperationException("Jwt:SecretKey no está configurada en appsettings.json");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>()
                    .LogError("Fallo de autenticación: {Message}, Token: {Token}, Cookie: {Cookie}",
                        context.Exception.Message,
                        context.Request.Headers["Authorization"],
                        context.Request.Cookies["JwtToken"]);
                context.Response.Redirect("/Auth/Login");
                return Task.CompletedTask;
            },
            OnMessageReceived = context =>
            {
                var token = context.Request.Cookies["JwtToken"];
                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            }
        };
    });

var app = builder.Build();

// Configuración del pipeline
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapControllers();

app.Run();