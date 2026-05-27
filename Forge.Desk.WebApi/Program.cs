using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Forge.Common.Interfaces;
using Forge.Common.Security;
using Forge.Database;
using Forge.Desk.WebApi.Configuration;
using Forge.Desk.WebApi.Security;
using Forge.Realtime;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularDev", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://localhost:4300")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<ForgeDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

AddJwtAuthentication(builder);
AddScoped(builder);

builder.Services.AddForgeRealtime(builder.Configuration);

var app = builder.Build();

CheckDatabaseCompatibility(app);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("AngularDev");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapForgeRealtime();

app.Run();

// Metoda do rejestracji serwisów o zakresie Scoped
void AddScoped(WebApplicationBuilder builder)
{
    builder.Services.AddScoped<IPasswordService, PasswordService>();
    builder.Services.AddScoped<ITokenService, TokenService>();
    builder.Services.AddScoped<DatabaseSeedData>();

    builder.Services.Configure<MenuOptions>(builder.Configuration.GetSection(MenuOptions.SectionName));
    builder.Services.AddSingleton<IMenuProvider, XmlMenuProvider>();
}

void AddJwtAuthentication(WebApplicationBuilder builder)
{
    var jwt = builder.Configuration.GetSection("Jwt");
    var key = jwt["Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured.");

    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwt["Issuer"],
                ValidAudience = jwt["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                ClockSkew = TimeSpan.FromMinutes(1)
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                    {
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                },
                OnTokenValidated = async context =>
                {
                    var sub = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    var sid = context.Principal?.FindFirst(JwtRegisteredClaimNames.Sid)?.Value;

                    if (!int.TryParse(sub, out var userId) || string.IsNullOrWhiteSpace(sid))
                    {
                        context.Fail("Token is missing required claims.");
                        return;
                    }

                    var sessions = context.HttpContext.RequestServices.GetRequiredService<ISessionRegistry>();
                    var current = await sessions.IsCurrentAsync(userId, sid, context.HttpContext.RequestAborted);
                    if (!current)
                    {
                        context.Fail("Session has been revoked.");
                    }
                }
            };
        });

    builder.Services.AddAuthorization();
}

// Metoda do sprawdzania kompatybilności bazy danych, wykonywania migracji i seeding danych
void CheckDatabaseCompatibility(WebApplication app)
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;

        var context = services.GetRequiredService<ForgeDbContext>();
        var seeder = services.GetRequiredService<DatabaseSeedData>();

        context.Database.Migrate();

        seeder.Seed(context);
    }
}
